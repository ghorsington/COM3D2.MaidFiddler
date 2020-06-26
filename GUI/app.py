import sys
from maidfiddler.util.config import load_config, CONFIG, save_config
from PyQt5.QtWidgets import QApplication, QStyleFactory
from maidfiddler.ui.main_window import MainWindow
from maidfiddler.util.logger import setup_logger, logger

def main():
    setup_logger()
    load_config()
    save_config()

    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    window = MainWindow()
    if CONFIG.getboolean("Options", "check_updates_on_startup", fallback=True):
        CONFIG["Options"]["check_updates_on_startup"] = "yes"
        save_config()
        window.check_updates(True)
    window.show()
    window.connect()

    app.exec_()
    logger.info("Exited!")

if __name__ == "__main__":
    main()
