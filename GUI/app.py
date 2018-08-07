import sys
import gevent
from maidfiddler.util.config import load_config, CONFIG
from PyQt5.QtWidgets import QApplication, QStyleFactory
import maidfiddler.util.util as util
from maidfiddler.ui.main_window import MainWindow

def event_loop(app):
    while util.APP_RUNNING:
        app.processEvents()
        gevent.sleep()

def close():
    util.APP_RUNNING = False

def main():
    # We set switch interval to change how often gevent switches 
    # between event polling and GUI.
    # This is **very** important, because Qt and ZeroRPC have to share 
    # the same thread.
    #
    # If switch interval is too low (0.005 by default), the UI doesn't have the time
    # to update, which causes laggy UI.
    # If it's too high, ZeroRPC will hog most of the processing time.
    # 60 Hz is an empirical value, but you might be able to get it lower.
    sys.setswitchinterval(1.0/60)
    group = gevent.pool.Group()

    print("Starting MF")

    load_config()

    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))
    group.spawn(event_loop, app)

    window = MainWindow(group, close)
    window.show()
    window.connect()

    group.join()


if __name__ == "__main__":
    main()
