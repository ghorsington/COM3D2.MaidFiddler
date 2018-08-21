import PyQt5.uic as uic
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtCore import Qt
import maidfiddler.util.util as util
from maidfiddler.util.translation import tr, tr_str
from maidfiddler.ui.resources import APP_ICON
import app_info as info

(ui_class, ui_base) = uic.loadUiType(
    open(util.get_resource_path("templates/about_dialog.ui")))


class AboutDialog(ui_class, ui_base):
    def __init__(self):
        super(AboutDialog, self).__init__()
        self.setupUi(self)
        self.close_button.clicked.connect(self.accept)
        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)

    def reload(self, core_version):
        icon = QPixmap()
        icon.loadFromData(APP_ICON)
        self.setWindowIcon(QIcon(icon))

        self.description_label.setText(tr(self.description_label))
        self.close_button.setText(tr(self.close_button))

        about_icon = QPixmap()
        about_icon.loadFromData(APP_ICON)
        self.icon_label.setPixmap(about_icon.scaled(32, 32))

        self.setWindowTitle(tr_str("about_dialog.title"))

        self.version_label.setText(tr(self.version_label))
        self.version.setText(f"GUI: {info.VERSION}\nCore: {core_version}")

        self.contributors_label.setText(tr(self.contributors_label))
        self.contributors.setText("\n".join(info.CONTRIBUTORS))

        self.project_label.setText(tr(self.project_label))
        self.project.setText(info.INFO_LINK)
