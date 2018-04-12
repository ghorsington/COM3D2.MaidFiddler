import zerorpc
import threading
import gevent

class EventPoller(object):
    def __init__(self, port):
        self.port = port
        self.event_handlers = dict()
        self.server = None
        self.ge_server = None

    def start(self, client, group):
        self.server = zerorpc.Server(self)
        self.server.bind("tcp://127.0.0.1:%s" % self.port)
        self.ge_server = group.spawn(self.server.run)
        client.SubscribeToEventHandler("tcp://127.0.0.1:%s" % self.port)
    
    def stop(self):
        gevent.kill(self.ge_server)

    def on(self, event_name, handler):
        if event_name not in self.event_handlers:
            self.event_handlers[event_name] = []
        self.event_handlers[event_name].append(handler)

    def emit(self, event_name, args):
        if event_name in self.event_handlers:
            for handler in self.event_handlers[event_name]:
                handler(args)
