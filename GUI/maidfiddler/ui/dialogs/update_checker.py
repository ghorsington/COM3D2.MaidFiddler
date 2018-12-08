import PyQt5.uic as uic
import time
import os
import threading
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QPushButton, QLabel, QApplication, QLayout, QSizePolicy
from PyQt5.QtCore import Qt, pyqtSignal, QThread
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON
import urllib.request as request
import json
import markdown2
import subprocess

from app_info import GIT_REPO, VERSION

DETACHED_PROCESS = 0x00000008
(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/updater_dialog.ui")))

class GetUpdateDataThread(QThread):
    update_available = pyqtSignal(dict)
    no_update = pyqtSignal()

    def __init__(self):
        QThread.__init__(self)
    
    def __del__(self):
        self.wait()
    
    def run(self):
        print("doing request")
        
        try:
            response = request.urlopen(f"https://api.github.com/repos/{GIT_REPO}/releases/latest", timeout=5)
        except Exception as e:
            print(e)
            self.no_update.emit()
            return

        print("request done")
        if response.getcode() != 200:
            print(f"Got status code: {response.getcode()}")
            self.no_update.emit()
            return
        data = json.load(response)
        version = data["tag_name"][1:]
        print(f"Latest version: {version}, current version: {VERSION}")
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
        print("Beginning download")

        tmp_downloads_path = util.get_resource_path("tmp_downloads")
        if not os.path.exists(tmp_downloads_path):
            os.mkdir(tmp_downloads_path)

        try:
            response = request.urlopen(self.url)
        except request.URLError as e:
            self.error.emit(f"Error downloading the file: {e}\nPlease try again later.")
            return

        with open(util.get_resource_path("tmp_downloads/installer.exe"), "wb") as f:
            while True:
                chunk = response.read(self.chunk_size)
                if not chunk:
                    break
                f.write(chunk)
                self.chunk_downloaded.emit()
        print("Download complete")
        self.download_complete.emit()

class UpdateDialog(ui_class, ui_base):
    def __init__(self):
        super(UpdateDialog, self).__init__()
        self.setupUi(self)
        self.layout().setSizeConstraint(QLayout.SetFixedSize)

        self.update_data_thread = GetUpdateDataThread()
        self.download_update_thread = DownloadUpdateThread()
       
        self.download_update_thread.chunk_downloaded.connect(self.on_chunk_downloaded)
        self.download_update_thread.download_complete.connect(self.run_installer)

        self.update_data_thread.update_available.connect(self.show_update_text)
        self.update_data_thread.no_update.connect(self.reject)

        self.downloadButton.clicked.connect(self.download_and_run)

        self.closeButton.clicked.connect(self.update_data_thread.exit)
        self.closeButton.clicked.connect(self.download_update_thread.exit)
        self.closeButton.clicked.connect(self.closeEvent)

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.setWindowTitle(tr_str("updater_dialog.title"))
        self.changelogBrowser.hide()
        self.progressLabel.hide()
        self.progressBar.hide()
        self.downloadButton.hide()

        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self.adjustSize()

    def run_installer(self):
        subprocess.Popen([util.get_resource_path("tmp_downloads/installer.exe")], creationflags=DETACHED_PROCESS)
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
        self.checkLabel.setText(f"A new update is available!\n\nYour version: {VERSION}\nAvailable version: {data['tag_name'][1:]}")
        self.changelogBrowser.setHtml(markdown2.markdown(data["body"]))
        self.changelogBrowser.show()
        self.downloadButton.show()
        self.adjustSize()

    def closeEvent(self, evt):
        print("kill")
        self.reject()

    def showEvent(self, evt):
        self.update_data_thread.start()


