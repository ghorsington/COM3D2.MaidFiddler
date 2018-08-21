from PyQt5.QtCore import QObject
from PyQt5.QtGui import QPixmap, QIcon
from PyQt5.QtWidgets import QListWidgetItem
from maidfiddler.ui.resources import NO_THUMBNAIL


class MaidsList(QObject):
    def __init__(self, ui):
        QObject.__init__(self)
        self.ui = ui
        self.maid_list = self.ui.maid_list
        self.maid_list_widgets = {}

    @property
    def core(self):
        return self.ui.core

    @property
    def maid_mgr(self):
        return self.ui.maid_mgr

    def init_events(self, event_poller):
        event_poller.on("deserialize_start", self.clear_list)
        event_poller.on("deserialize_done", self.save_changed)
        event_poller.on("maid_added", lambda a: self.add_maid(a["maid"]))
        event_poller.on("maid_removed", self.on_maid_removed)
        event_poller.on("maid_thumbnail_changed", self.thumb_changed)
        event_poller.on("maid_prop_changed", self.prop_changed)
        event_poller.on("old_maid_deserialized", self.fix_maid_data)

        self.maid_list.currentItemChanged.connect(self.maid_selected)

        # All maids
        self.ui.actionUnlock_value_ranges.toggled.connect(
            lambda c: self.core.SetUnlockRanges(c))
        self.ui.actionAll_unlock_yotogi_skills.toggled.connect(
            lambda c: self.core.SetShowAllYotogiSkills(c))
        self.ui.actionAll_unlock_yotogi_commands.toggled.connect(
            lambda c: self.core.SetEnableAllYotogiCommand(c))
        self.ui.actionUnlock_all_scenarios.toggled.connect(
            lambda c: self.core.SetEnableAllScenarios(c))
        self.ui.actionUnlock_all_schedule_events.toggled.connect(
            lambda c: self.core.SetEnableAllScheduleItems(c))
        self.ui.actionMax_all_maids.triggered.connect(
            self.do_and_reselect(lambda: self.core.MaxAllForAllMaids()))
        self.ui.actiontop_bar_all_maid_unlock_max_stats.triggered.connect(
            self.do_and_reselect(lambda: self.core.UnlockAllForAllMaids()))

        # "Unlock all" for current maid (easier to have here)
        self.ui.actiontop_bar_cur_maid_unlock_all.triggered.connect(
            self.do_and_reselect(lambda: self.core.UnlockAllActive(False)))
        self.ui.actiontop_bar_cur_maid_set_max_all.triggered.connect(
            self.do_and_reselect(lambda: self.core.UnlockAllActive(True)))
        self.ui.actionCur_maid_max_all.triggered.connect(
            self.do_and_reselect(lambda: self.core.MaxAllActive()))

        self.ui.actiontop_bar_cur_maid_unlock_noon_class.triggered.connect(
            lambda: self.core.UlockAllJobClassActive(False))
        self.ui.actiontop_bar_cur_maid_unlock_yotogi_class.triggered.connect(
            lambda: self.core.UnlockAllYotogiClassActive(False))
        self.ui.actiontop_bar_cur_maid_max_job.triggered.connect(
            lambda: self.core.UlockAllJobClassActive(True))
        self.ui.actiontop_bar_cur_maid_max_yotogi_job.triggered.connect(
            lambda: self.core.UnlockAllYotogiClassActive(True))

    def do_and_reselect(self, work):
        def handler():
            if work():
                self.maid_selected(self.ui.maid_list.currentItem(), None)
        return handler

    def prop_changed(self, args):
        if args["guid"] not in self.maid_list_widgets:
            return

        maid_data = self.maid_mgr.maid_data[args["guid"]]

        if args["property_name"] not in maid_data:
            return

        maid_data[args["property_name"]] = args["value"]
        self.maid_list_widgets[args["guid"]].setText(
            f"{maid_data['firstName']} {maid_data['lastName']}")

    def maid_selected(self, n, p):
        if n is None:
            self.ui.ui_tabs.setEnabled(False)
            self.ui.menuSelected_maid.setEnabled(False)
            self.maid_mgr.selected_maid = None
            return

        self.ui.ui_tabs.setEnabled(True)
        self.ui.menuSelected_maid.setEnabled(True)

        maid = self.core.SelectActiveMaid(n.guid)

        if maid is None:
            return

        self.maid_mgr.maid_data[n.guid]["firstName"] = maid["properties"]["firstName"]
        self.maid_mgr.maid_data[n.guid]["lastName"] = maid["properties"]["lastName"]
        self.maid_mgr.maid_data[n.guid]["thumbnail"] = maid["maid_thumbnail"]
        self.maid_mgr.selected_maid = maid

        for tab in self.ui.tabs:
            tab.on_maid_selected()

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

        item = MaidListItem(
            QIcon(thumb), self.get_display_name(maid), maid["guid"])

        self.maid_list_widgets[maid["guid"]] = item
        self.maid_list.addItem(self.maid_list_widgets[maid["guid"]])

    def fix_maid_data(self, args):
        self.maid_mgr.update_guid(args["old_guid"], args["new_guid"])

        item = self.maid_list_widgets[args["old_guid"]]
        del self.maid_list_widgets[args["old_guid"]]

        text = self.get_display_name(args)
        item.text = text
        item.setText(text)
        item.guid = args["new_guid"]

        thumb = QPixmap()
        thumb.loadFromData(args["thumbnail"])
        item.setIcon(QIcon(thumb))

        self.maid_list_widgets[args["new_guid"]] = item

    def reload_maids(self):
        self.clear_list()
        maids = self.core.GetAllStockMaidsBasic()

        for maid in maids:
            self.add_maid(maid)

    def thumb_changed(self, args):
        if args["thumb"] is None or args["guid"] not in self.maid_list_widgets:
            return

        pixmap = QPixmap()
        pixmap.loadFromData(args["thumb"])

        self.maid_list_widgets[args["guid"]].setIcon(QIcon(pixmap))

    def on_maid_removed(self, args):
        guid = args["maid_id"]
        if guid not in self.maid_list_widgets:
            return

        print("Maid removed!")

        self.maid_list.takeItem(
            self.maid_list.row(self.maid_list_widgets[guid]))
        del self.maid_list_widgets[guid]
        self.maid_mgr.remove_maid(guid)

    def get_display_name(self, maid):
        return f"{maid['firstName']} {maid['lastName']}"


class MaidListItem(QListWidgetItem):
    def __init__(self, icon, text, guid):
        QListWidgetItem.__init__(self, icon, text)
        self.text = text
        self.guid = guid

    def __lt__(self, other):
        return self.text.lower() < other.text.lower()
