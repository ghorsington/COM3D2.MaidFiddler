import sys
import zerorpc
import windows.main_window as main_window
from PyQt5.QtWidgets import QApplication, QStyleFactory
import gevent
import threading
import util.util as util

client = None
group = gevent.pool.Group()

def connect():
    global client

    print("Connecting to tcp://127.0.0.1:8899")
    client = zerorpc.Client()
    client.connect("tcp://127.0.0.1:8899")
    try:
        client._zerorpc_ping()
    except Exception as ex:
        print("Failed to connect because " + str(ex))
    print("Connected!")

def event_loop(app):
    timer = 0
    while util.APP_RUNNING:
        app.processEvents()
        gevent.sleep()

        """ timer += 1
        if timer % 800 == 0:
            timer = 0
            gevent.sleep() """

def close():
    util.APP_RUNNING = False

def main():
    global group
    gevent.setswitchinterval(0.1)

    print("Starting MF")
    connect()

    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    window = main_window.MainWindow(client, group, close)
    
    window.show()
    group.spawn(event_loop, app)
    group.join()


if __name__ == "__main__":
    main()
