import sys
import zerorpc
import windows.main_window as main_window
from PyQt5.QtWidgets import QApplication, QStyleFactory
import gevent
import util.util as util

def connect():
    print("Connecting to tcp://127.0.0.1:8899")
    client = zerorpc.Client()
    client.connect("tcp://127.0.0.1:8899")
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
    gevent.setswitchinterval(1.0/60)
    group = gevent.pool.Group()

    print("Starting MF")
    client = connect()

    if client is None:
        sys.exit(1)

    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    window = main_window.MainWindow(client, group, close)
    
    window.show()
    group.spawn(event_loop, app)
    group.join()


if __name__ == "__main__":
    main()
