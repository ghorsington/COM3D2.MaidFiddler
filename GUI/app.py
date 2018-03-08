import sys
import os
import PyQt5.uic as uic
from PyQt5.QtWidgets import QApplication, QMainWindow, QStyleFactory

ui_template = "maid_fiddler.ui"

if getattr(sys, "_MEIPASS", False):
    ui_template = os.path.join(getattr(sys, "_MEIPASS"), ui_template)

app = QApplication(sys.argv)
ui = uic.loadUi(open(ui_template))

app.setStyle(QStyleFactory.create("Fusion"))

ui.show()
sys.exit(app.exec_())
