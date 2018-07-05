import sys
import gevent
import zerorpc
from PyQt5.QtWidgets import QApplication, QStyleFactory
import maidfiddler.util.util as util
from maidfiddler.ui.main_window import MainWindow


def connect():
    print(f"Connecting to tcp://{util.GAME_ADDRESS}:8899")
    client = zerorpc.Client()
    client.connect(f"tcp://{util.GAME_ADDRESS}:8899")
    try:
        client._zerorpc_ping()
        return client
    except Exception as ex:
        print("Failed to connect because " + str(ex))
        client.close()
        return None
    print("Connected!")


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
    client = connect()

    if client is None:
        sys.exit(1)

    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    window = MainWindow(client, group, close)

    window.show()
    group.spawn(event_loop, app)
    group.join()


if __name__ == "__main__":
    main()
