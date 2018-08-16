import PyQt5.uic as uic
import traceback
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtCore import Qt
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON, ERR_ICON

(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/error_dialog.ui")))

class ErrorDialog(ui_class, ui_base):
    def __init__(self, t, error, tb):
        super(ErrorDialog, self).__init__()
        self.setupUi(self)

        self.error = error
        self.traceback = tb
        self.t = t

        self.close_button.clicked.connect(self.accept)

        self.load()

    def load(self):
        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.description_label.setText(tr(self.description_label))
        self.close_button.setText(tr(self.close_button))
        
        error_icon = QPixmap()
        error_icon.loadFromData(ERR_ICON)
        self.icon_label.setPixmap(error_icon)

        self.setWindowTitle(tr_str("error_dialog.title"))
        self.error_text.setPlainText("".join(traceback.format_exception(self.t, self.error, self.traceback)))

        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)