import PyQt5.uic as uic
import time
import threading
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QPushButton, QLabel, QApplication
from PyQt5.QtCore import Qt
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON

(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/connect_dialog.ui")))


class ConnectDialog(ui_class, ui_base):
    def __init__(self, main_window, core):
        super(ConnectDialog, self).__init__()
        self.setupUi(self)

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.main_window = main_window
        self.core = core
        self.game_data = None
        self.worker = None
        self.run_connect = False

        self.close_button.clicked.connect(self.closeEvent)

        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)

    def reload(self):
        for label in self.findChildren(QLabel):
            label.setText(tr(label))

        for button in self.findChildren(QPushButton):
            button.setText(tr(button))

        self.setWindowTitle(tr_str("connect_dialog.title"))
        self.status_label.setStyleSheet("color: black;")
        self.status_label.setText(tr_str("connect_dialog.status.wait"))

    def showEvent(self, evt):
        self.try_connect()

    def closeEvent(self, evt):
        self.run_connect = False
        self.worker.join()
        self.reject()

    def connected(self):
        self.status_label.setStyleSheet("color: green;")
        self.status_label.setText(tr_str("connect_dialog.status.dl_info"))
        self.game_data, err = self.core.try_invoke("GetGameInfo")
        if not err:
            self.run_connect = False
            self.accept()
        else:
            self.status_label.setStyleSheet("color: orange;")
            self.status_label.setText(tr_str("connect_dialog.status.connect"))
        return not err

    def begin_connect(self):
        while self.run_connect:
            time.sleep(1)
            try:
                self.core.connect("MaidFiddlerService")
                if self.connected():
                    print("Connected!")
                    return
            except:
                print("Failed to connect! Retrying in a second!")

    def try_connect(self):
        self.status_label.setStyleSheet("color: orange;")
        self.status_label.setText(tr_str("connect_dialog.status.connect"))

        self.worker = threading.Thread(target=self.begin_connect)
        self.run_connect = True
        self.worker.start()
