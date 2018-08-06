from PyQt5.QtCore import Qt, QObject

class UiTab(QObject):
    def __init__(self, ui):
        QObject.__init__(self)
        self.ui = ui
        self._game_data = None

    @property
    def game_data(self):
        return self._game_data

    @property
    def core(self):
        return self.ui.core

    @property
    def maid_mgr(self):
        return self.ui.maid_mgr

    @game_data.setter
    def game_data(self, value):
        self._game_data = value
        self.update_ui()

    def init_events(self, event_poller):
        pass

    def on_maid_selected(self):
        pass

    def update_ui(self):
        raise NotImplementedError()

    def translate_ui(self):
        pass
