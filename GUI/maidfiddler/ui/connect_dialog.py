import PyQt5.uic as uic
import time
import threading
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QPushButton, QLabel, QApplication
from PyQt5.QtCore import Qt, QThread, pyqtSignal
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON

(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/connect_dialog.ui")))


class ConnectWorker(QThread):
    connected = pyqtSignal()
    setup_complete = pyqtSignal(dict)
    connection_reset = pyqtSignal()

    def __init__(self, core):
        QThread.__init__(self)
        self.connecting = False
        self.core = core

    def __del__(self):
        self.wait()

    def _connected(self):
        self.connected.emit()
        print("Got connection! Trying to get game data!")
        try:
            game_data, err = self.core.try_invoke("GetGameInfo")
        except Exception as e:
            print(f"Got error while calling GetGameInfo: {e}")
            self.connection_reset.emit()
            return False
        self.connecting = False
        self.setup_complete.emit(game_data)
        return True

    def run(self):
        self.connecting = True
        while self.connecting:
            QThread.sleep(1)
            try:
                self.core.connect("MaidFiddlerService")
                if self._connected():
                    print("Connected!")
                    return
            except Exception as e:
                print(f"Failed to connect because {e}! Retrying in a second!")
                self.connection_reset.emit()


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

        self.worker = ConnectWorker(core)
        self.worker.connected.connect(self.on_connected)
        self.worker.connection_reset.connect(self.on_connection_reset)
        self.worker.setup_complete.connect(self.on_setup_complete)

        self.close_button.clicked.connect(self.closeEvent)

        self.setWindowFlags(self.windowFlags() & ~
                            Qt.WindowContextHelpButtonHint)

        self.reload()

    def on_setup_complete(self, game_data):
        self.game_data = game_data
        self.accept()

    def on_connected(self):
        self.status_label.setStyleSheet("color: green;")
        self.status_label.setText(tr_str("connect_dialog.status.dl_info"))

    def on_connection_reset(self):
        self.status_label.setStyleSheet("color: orange;")
        self.status_label.setText(tr_str("connect_dialog.status.connect"))

    def reload(self):
        for label in self.findChildren(QLabel):
            label.setText(tr(label))

        for button in self.findChildren(QPushButton):
            button.setText(tr(button))

        self.setWindowTitle(tr_str("connect_dialog.title"))
        self.status_label.setStyleSheet("color: black;")
        self.status_label.setText(tr_str("connect_dialog.status.wait"))

    def showEvent(self, evt):
        self.status_label.setStyleSheet("color: orange;")
        self.status_label.setText(tr_str("connect_dialog.status.connect"))

        self.worker.start()

    def closeEvent(self, evt):
        self.worker.connecting = False
        self.worker.exit()
        self.reject()
