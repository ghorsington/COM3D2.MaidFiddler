import PyQt5.uic as uic
import os
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QLayout
from PyQt5.QtCore import Qt, pyqtSignal, QThread
import maidfiddler.util.util as util
from maidfiddler.util.config import CONFIG, save_config
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON
from maidfiddler.util.logger import logger
import urllib.request as request
from urllib.error import URLError
import json
import markdown2
import subprocess

from app_info import GIT_REPO, VERSION

TMP_FOLDER = "tmp_downloads"
UPDATER_FILE = "installer.exe"

DETACHED_PROCESS = 0x00000008
(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/updater_dialog.ui")))

class GetUpdateDataThread(QThread):
    update_available = pyqtSignal(dict)
    no_update = pyqtSignal()
    error = pyqtSignal(str)

    def __init__(self):
        QThread.__init__(self)
    
    def __del__(self):
        self.wait()
    
    def run(self):
        logger.debug("doing request")
        
        try:
            response = request.urlopen(f"https://api.github.com/repos/{GIT_REPO}/releases/latest", timeout=5)
        except URLError as e:
            self.error.emit(str(e.reason))
            return
        data = json.load(response)
        version = data["tag_name"][1:]
        logger.info(f"Latest version: {version}, current version: {VERSION}")
        if version <= VERSION:
            self.no_update.emit()
            return

        self.update_available.emit(data)

class DownloadUpdateThread(QThread):
    chunk_downloaded = pyqtSignal()
    download_complete = pyqtSignal()
    error = pyqtSignal(str)
    chunk_size = 1024

    def __init__(self):
        QThread.__init__(self)
        self.url = ""

    def __del__(self):
        self.wait()

    def setUrl(self, url):
        self.url = url

    def run(self):
        logger.debug("Beginning download")

        tmp_downloads_path = os.path.join(util.BASE_DIR, TMP_FOLDER)
        if not os.path.exists(tmp_downloads_path):
            os.mkdir(tmp_downloads_path)

        try:
            response = request.urlopen(self.url)
        except URLError as e:
            self.error.emit(str(e.reason))
            return

        with open(os.path.join(tmp_downloads_path, UPDATER_FILE), "wb") as f:
            while True:
                chunk = response.read(self.chunk_size)
                if not chunk:
                    break
                f.write(chunk)
                self.chunk_downloaded.emit()
        logger.debug("Download complete")
        self.download_complete.emit()

class UpdateDialog(ui_class, ui_base):
    def __init__(self, silent):
        super(UpdateDialog, self).__init__()
        self.setupUi(self)
        self.layout().setSizeConstraint(QLayout.SetFixedSize)
        
        self.silent = silent

        self.update_data_thread = GetUpdateDataThread()
        self.download_update_thread = DownloadUpdateThread()
       
        self.download_update_thread.chunk_downloaded.connect(self.on_chunk_downloaded)
        self.download_update_thread.download_complete.connect(self.run_installer)
        self.download_update_thread.error.connect(self.on_downloader_error)

        self.update_data_thread.update_available.connect(self.show_update_text)
        self.update_data_thread.no_update.connect(self.on_no_update)
        self.update_data_thread.error.connect(self.on_updater_error)

        self.downloadButton.clicked.connect(self.download_and_run)

        self.closeButton.clicked.connect(self.update_data_thread.exit)
        self.closeButton.clicked.connect(self.download_update_thread.exit)
        self.closeButton.clicked.connect(self.closeEvent)

        check_on_startup = CONFIG.getboolean("Options", "check_updates_on_startup", fallback=True)
        CONFIG["Options"]["check_updates_on_startup"] = "yes" if check_on_startup else "no"
        save_config()
        self.checkOnStartupCheckBox.setCheckState(Qt.Checked if check_on_startup else Qt.Unchecked)
        self.checkOnStartupCheckBox.stateChanged.connect(self.on_startup_check_option_change)

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.setWindowTitle(tr_str("updater_dialog.title"))
        self.checkLabel.setText(tr_str("updater_dialog.checking_updates"))
        self.checkOnStartupCheckBox.setText(tr(self.checkOnStartupCheckBox))
        self.downloadButton.setText(tr(self.downloadButton))
        self.closeButton.setText(tr(self.closeButton))

        self.changelogBrowser.hide()
        self.progressLabel.hide()
        self.progressBar.hide()
        self.downloadButton.hide()

        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self.adjustSize()

    def on_startup_check_option_change(self, state):
        opt = "yes" if state == Qt.Checked else "no"
        CONFIG["Options"]["check_updates_on_startup"] = opt
        save_config()

    def on_downloader_error(self, msg):
        self.progressBar.setValue(0)
        self.progressLabel.setText(tr_str("updater_dialog.download_error").format(msg))
        self.adjustSize()

    def on_updater_error(self, msg):
        if self.silent:
            self.reject()
        else:
            self.checkLabel.setText(tr_str("updater_dialog.updater_error").format(msg))
            self.adjustSize()

    def on_no_update(self):
        if self.silent:
            self.reject()
        else:
            self.checkLabel.setText(tr_str("updater_dialog.no_update"))
            self.adjustSize()

    def run_installer(self):
        subprocess.Popen([os.path.join(util.BASE_DIR, TMP_FOLDER, UPDATER_FILE)], creationflags=DETACHED_PROCESS)
        self.accept()

    def on_chunk_downloaded(self):
        self.progressBar.setValue(self.progressBar.value() + self.download_update_thread.chunk_size)
        downloaded_mb = self.progressBar.value() / 1000000.0
        total_mb = self.progressBar.maximum() / 1000000.0
        self.progressLabel.setText(f"{downloaded_mb:.3f} / {total_mb:.3f} MB")

    def download_and_run(self):
        self.progressLabel.show()
        self.progressBar.show()
        self.downloadButton.setEnabled(False)
        self.download_update_thread.start()

    def show_update_text(self, data):
        self.download_update_thread.setUrl(data["assets"][0]["browser_download_url"])
        self.progressBar.setMaximum(data["assets"][0]["size"])
        self.checkLabel.setText(tr_str("updater_dialog.update_available").format(VERSION, data['tag_name'][1:]))
        self.changelogBrowser.setHtml(markdown2.markdown(data["body"]))
        self.changelogBrowser.show()
        self.downloadButton.show()
        self.adjustSize()

    def closeEvent(self, evt):
        self.reject()

    def showEvent(self, evt):
        self.update_data_thread.start()