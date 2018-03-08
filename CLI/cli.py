import zerorpc
import rlcompleter
import readline
import json

__version__ = "1.0.0.0"

client = None
methods = []
method_set = set()


def start_info():
    print("#### Maid Fiddler CLI ####")
    print("#### Version %s  ####" % __version__)
    print("")
    print("This a tool to control Maid Fiddler Core through a command-line interface (CLI).\n")
    print("To use the tool:")
    print("1. Install Maid Fiddler Core")
    print("2. Start COM3D2")
    print("3. Look for the CONNECTION PORT in the game's console.")
    print("")
    print("Please write the port and press ENTER to connect.")

    valid = False

    while not valid:
        port = input("Input the port (default: 8899): ")

        if port == "":
            port = "8899"

        try:
            port = int(port)
        except:
            print("The port must be an integer!")
            continue

        if port < 0 or port > 65536:
            print("The port number must be in range 0-65536")
            continue

        valid = connect(port)

    print("Connected!\n")


def connect(port):
    global client
    print("Trying to connect to tcp://127.0.0.1:%d" % port)

    try:
        client = zerorpc.Client()
        client.connect("tcp://127.0.0.1:%d" % port)
        client._zerorpc_ping()
    except Exception as ex:
        print("Failed to connect because: " + str(ex))
        client.close()
        client = None
        print("Client set to none!")
        return False

    return True


def init_completer():
    global methods, method_set
    methods = client._zerorpc_list()
    method_set = set(methods)

    def complete(text, steps):
        m = [method for method in methods if method.startswith(text)]
        if steps < len(m):
            return m[steps]
        else:
            return None
    readline.set_completer(complete)
    readline.parse_and_bind("tab: complete")


def print_help(command):
    if command not in method_set:
        print("Command not found.")
        return

    try:
        doc_string = client._zerorpc_help(command)
    except Exception as e:
        print("Cannot get documentation: " + str(e))
        return

    print("Documentation for %s:" % command)
    print(doc_string)


def main_loop():
    init_completer()
    print("You can now control Maid Fiddler Core!")
    print("Press TAB to toggle autocomplete!")
    print("Use :h <command> to get help for the command!")

    while True:
        try:
            s = input(">>> ")
        except (EOFError, KeyboardInterrupt):
            return

        if s.strip() == "":
            continue

        if s.startswith(":h"):
            print_help(s[2:].strip())
        else:
            parts = s.split()
            method_name = parts[0]
            if method_name in method_set:
                parsed_args = json.loads("[%s]" % (",".join(parts[1:])))
                try:
                    r = client.__call__(method_name, *parsed_args)
                    print(r)
                except zerorpc.RemoteError as e:
                    print("Error %s: %s. \nStack trace:\n%s" % (e.name, e.msg, e.traceback))
            else:
                print("No such command: %s" % method_name)


def main():
    start_info()
    main_loop()


if __name__ == '__main__':
    main()
