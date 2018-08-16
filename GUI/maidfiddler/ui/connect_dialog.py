import PyQt5.uic as uic
import sys
import zerorpc
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QPushButton, QLabel
from PyQt5.QtCore import Qt
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.util.config import CONFIG, save_config
from maidfiddler.ui.resources import APP_ICON

(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/connect_dialog.ui")))

class ConnectDialog(ui_class, ui_base):
    def __init__(self, main_window):
        super(ConnectDialog, self).__init__()
        self.setupUi(self)

        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.main_window = main_window
        self.client = None

        self.connect_button.clicked.connect(self.try_connect)
        self.close_button.clicked.connect(self.closeEvent)

        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)

    def reload(self):
        self.connect_button.setEnabled(True)
        self.port.setEnabled(True)
        self.port.setValue(CONFIG.getint("Connection", "port", fallback=8899))

        for label in self.findChildren(QLabel):
            label.setText(tr(label))

        for button in self.findChildren(QPushButton):
            button.setText(tr(button))

        self.setWindowTitle(tr_str("connect_dialog.title"))
        self.status_label.setStyleSheet("color: black;")
        self.status_label.setText(tr_str("connect_dialog.status.wait"))
        self.client = None

    def closeEvent(self, evt):
        sys.exit(0)

    def try_connect(self):
        self.connect_button.setEnabled(False)
        self.port.setEnabled(False)
        self.client = zerorpc.Client()
        self.status_label.setStyleSheet("color: orange;")
        self.status_label.setText(tr_str("connect_dialog.status.connect").format(util.GAME_ADDRESS, self.port.value()))
        
        try:
            self.client.connect(f"tcp://{util.GAME_ADDRESS}:{self.port.value()}")
            self.client._zerorpc_ping()
        except Exception as ex:
            self.status_label.setStyleSheet("color: red;")
            self.status_label.setText(tr_str("connect_dialog.status.fail").format(str(ex)))
            self.client.close()
            self.client = None
            self.connect_button.setEnabled(True)
            self.port.setEnabled(True)
            return
        
        CONFIG["Connection"]["port"] = str(self.port.value())
        save_config()
        self.accept()

