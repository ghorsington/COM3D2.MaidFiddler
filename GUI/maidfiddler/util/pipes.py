import struct
import msgpack
import threading


class RemoteError(Exception):

    def __init__(self, name, human_msg, human_traceback):
        Exception.__init__(self)
        self.name = name
        self.msg = human_msg
        self.traceback = human_traceback

    def __str__(self):
        return f"{self.name}: {self.msg}\n{self.traceback}"


class PipeRpcCaller:
    def __init__(self, on_connection_lost):
        self.handler = None
        self.call_id = 0
        self.on_connection_lost = on_connection_lost

    def connect(self, pipe_name):
        self.handler = open(f"\\\\.\\pipe\\{pipe_name}", "r+b", 0)
        self._flush()

    def is_connected(self):
        return self.handler is not None

    def _flush(self):
        try:
            # Send a dummy ping call, but read all of the steam until EOF
            # This forces the stream to be reset
            # Yes, it slows initial load a bit, but this is fine in our case
            packed = msgpack.packb(
                {"msg_id": 0, "data": ["ping", {"pong": False}]})
            self.handler.write(struct.pack("I", len(packed)))
            self.handler.write(packed)
            self.handler.read(2**32)
        except:
            print(f"Flushed!")

    def close(self):
        try:
            h = self.handler
            self.handler = None
            h.close()
            self.call_id = 0
            print("Handler closed!")
        except Exception as e:
            print("Error while closing the handler!")
            print(e)

    def __call__(self, method, *args, **kargs):
        result, err = self.try_invoke(method, *args)
        if err:
            self.on_connection_lost.emit()
        return result

    def try_invoke(self, method, *args):
        if isinstance(method, bytes):
            method = method.decode('utf-8')
        print(f"Calling {method}")

        self.call_id += 1

        obj = {
            "msg_id": self.call_id,
            "data": [
                "call",
                {
                    "method": method,
                    "args": args
                }
            ]
        }

        packed = msgpack.packb(obj)

        try:
            self.handler.write(struct.pack("I", len(packed)))
            self.handler.write(packed)
            # Advance the cursor by reading the packed bytes
            # Has to be done because it's not advanced automatically for some reason
            self.handler.read(len(packed))

            response_len = struct.unpack("I", self.handler.read(4))[0]
            response_packed = self.handler.read(response_len)
        except:
            self.close()
            return (None, True)

        response = msgpack.unpackb(response_packed, raw=False)

        response_type = response["data"][0]

        if response_type == "response":
            return (response["data"][1]["result"], False)
        elif response_type == "error":
            error_info = response["data"][1]
            raise RemoteError(
                error_info["err_name"], error_info["err_message"], error_info["stack_trace"])
        else:
            raise RemoteError("InvalidResponse",
                              "The response from the method is invalid", "")

    def __getattr__(self, method):
        return lambda *args, **kargs: self(method, *args, **kargs)


class PipedEventHandler:
    def __init__(self, name, on_connection_lost):
        self.name = name
        self.on_connection_lost = on_connection_lost
        self.event_handlers = {}
        self.running = False
        self.event_loop = None

    def start_polling(self):
        if self.running:
            return
        self.running = True
        self.event_loop = threading.Thread(target=self._loop)
        self.event_loop.start()

    def stop_polling(self):
        if not self.running:
            return
        self.running = False
        self.event_loop.join()
        self.event_loop = None

    def on(self, event, handler):
        if event not in self.event_handlers:
            self.event_handlers[event] = [handler]
        else:
            self.event_handlers[event].append(handler)

    def _loop(self):
        print(f"Connecting to {self.name}")

        try:
            f = None
            f = open(f"\\\\.\\pipe\\{self.name}", "r+b", 0)

            print("Connected event handler!")

            while self.running:
                n = struct.unpack("I", f.read(4))[0]
                data = f.read(n)
                obj = msgpack.unpackb(data, raw=False)
                if obj["data"][0] == "call" and obj["data"][1]["method"] == "emit":
                    args = obj["data"][1]["args"][0]

                    for evt_args in args:
                        print(
                            f"Event {evt_args['event_name']}")
                        if evt_args["event_name"] in self.event_handlers:
                            for handler in self.event_handlers[evt_args["event_name"]]:
                                handler.emit(evt_args["args"])

            f.close()
        except:
            if f is not None:
                f.close()
            self.running = False
            self.event_loop = None
            self.on_connection_lost.emit()
