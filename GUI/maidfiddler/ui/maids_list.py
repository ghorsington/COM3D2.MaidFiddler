from PyQt5.QtCore import QObject
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QListWidgetItem
from maidfiddler.ui.resources import NO_THUMBNAIL

MAID_GUID = 33

class MaidsList(QObject):
    def __init__(self, ui, core, maid_mgr):
        QObject.__init__(self)
        self.ui = ui
        self.core = core
        self.maid_mgr = maid_mgr
        self.maid_list = self.ui.maid_list
        self.maid_list_widgets = {}

    def init_events(self, event_poller):
        event_poller.on("deserialize_start", self.clear_list)
        event_poller.on("deserialize_done", self.save_changed)
        event_poller.on("maid_added", lambda a: self.add_maid(a["maid"]))
        event_poller.on("maid_thumbnail_changed", self.thumb_changed)

        self.maid_list.currentItemChanged.connect(self.maid_selected)

    def maid_selected(self, n, p):
        self.ui.ui_tabs.setEnabled(n is not None)
        next_data = n.data(MAID_GUID) if n else ""
        print(f"Selected maid: {next_data}")

    def clear_list(self, аrgs=None):
        self.ui.ui_tabs.setEnabled(False)
        self.maid_mgr.clear()
        self.maid_list.clear()
        self.maid_list_widgets.clear()

    def save_changed(self, args):
        if not args["success"]:
            return
        self.reload_maids()

    def add_maid(self, maid):
        self.maid_mgr.add_maid(maid)

        if "maid_thumbnail" in maid and maid["maid_thumbnail"] is not None:
            thumb_image = maid["maid_thumbnail"]
        else:
            thumb_image = NO_THUMBNAIL

        thumb = QPixmap()
        thumb.loadFromData(thumb_image)

        item = QListWidgetItem(QIcon(thumb), f"{maid['set_properties']['firstName']} {maid['set_properties']['lastName']}")
        item.setData(MAID_GUID, maid["guid"])

        self.maid_list_widgets[maid["guid"]] = item
        self.maid_list.addItem(self.maid_list_widgets[maid["guid"]])

    def reload_maids(self):
        maids = self.core.GetAllStockMaids()

        for maid in maids:
            self.add_maid(maid)

    def thumb_changed(self, args):
        if args["thumb"] is None or args["guid"] not in self.maid_list_widgets:
            return

        pixmap = QPixmap()
        pixmap.loadFromData(args["thumb"])

        self.maid_list_widgets[args["guid"]].setIcon(QIcon(pixmap))
