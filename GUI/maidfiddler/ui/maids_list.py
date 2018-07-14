from PyQt5.QtCore import QObject
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QListWidgetItem
from maidfiddler.ui.resources import NO_THUMBNAIL

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
        event_poller.on("maid_prop_changed", self.prop_changed)

        self.maid_list.currentItemChanged.connect(self.maid_selected)
        self.ui.actionSort_maids_by_guid.triggered.connect(self.sort_test)

    def sort_test(self):
        self.maid_list.sortItems()

    def prop_changed(self, args):
        if args["guid"] not in self.maid_list_widgets:
            return

        maid_data = self.maid_mgr.maid_data[args["guid"]]

        if args["property_name"] not in maid_data:
            return

        maid_data[args["property_name"]] = args["value"]
        self.maid_list_widgets[args["guid"]].setText(f"{maid_data['firstName']} {maid_data['lastName']}")

    def maid_selected(self, n, p):
        if n is None:
            self.ui.ui_tabs.setEnabled(False)
            self.maid_mgr.selected_maid = None
            return
        
        self.ui.ui_tabs.setEnabled(True)

        maid = self.core.SelectActiveMaid(n.guid)
        
        if maid is None:
            return

        self.maid_mgr.maid_data[n.guid]["firstName"] = maid["properties"]["firstName"]
        self.maid_mgr.maid_data[n.guid]["lastName"] = maid["properties"]["lastName"]
        self.maid_mgr.maid_data[n.guid]["thumbnail"] = maid["maid_thumbnail"]
        self.maid_mgr.selected_maid = maid

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

        if "thumbnail" in maid and maid["thumbnail"] is not None:
            thumb_image = maid["thumbnail"]
        else:
            thumb_image = NO_THUMBNAIL

        thumb = QPixmap()
        thumb.loadFromData(thumb_image)

        item = MaidListItem(QIcon(thumb), f"{maid['firstName']} {maid['lastName']}", maid["guid"])

        self.maid_list_widgets[maid["guid"]] = item
        self.maid_list.addItem(self.maid_list_widgets[maid["guid"]])

    def reload_maids(self):
        maids = self.core.GetAllStockMaidsBasic()

        for maid in maids:
            self.add_maid(maid)

    def thumb_changed(self, args):
        if args["thumb"] is None or args["guid"] not in self.maid_list_widgets:
            return

        pixmap = QPixmap()
        pixmap.loadFromData(args["thumb"])

        self.maid_list_widgets[args["guid"]].setIcon(QIcon(pixmap))


class MaidListItem(QListWidgetItem):
    def __init__(self, icon, text, guid):
        QListWidgetItem.__init__(self, icon, text)
        self.text = text
        self.guid = guid
    
    def __lt__(self, other):
        return self.text.lower() < other.text.lower()
