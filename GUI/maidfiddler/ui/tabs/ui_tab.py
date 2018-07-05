from PyQt5.QtCore import Qt, QObject

class UiTab(QObject):
    def __init__(self, ui, core, maid_mgr):
        QObject.__init__(self)
        self.ui = ui
        self.core = core
        self.maid_mgr = maid_mgr
        self._game_data = None

    @property
    def game_data(self):
        return self._game_data

    @game_data.setter
    def game_data(self, value):
        self._game_data = value
        self.update_ui()

    def init_events(self, event_poller):
        pass

    def update_ui(self):
        raise NotImplementedError()