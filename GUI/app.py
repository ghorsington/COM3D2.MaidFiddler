import sys
import zerorpc
import windows.main_window as main_window
from PyQt5.QtWidgets import QApplication, QStyleFactory
import gevent

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
    def loop(app):
        app.processEvents()
    while True:
        looper = group.spawn(loop, app)
        looper.join()

def main():
    global group
    print("Starting MF")
    connect()
    app = QApplication(sys.argv)
    app.setStyle(QStyleFactory.create("Fusion"))

    window = main_window.MainWindow(client, group)
    
    window.ui.show()
    group.spawn(event_loop, app)
    group.join()
    sys.exit(0)


if __name__ == "__main__":
    main()
