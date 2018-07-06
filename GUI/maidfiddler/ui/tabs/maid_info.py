from PyQt5.QtCore import Qt
from .ui_tab import UiTab
from maidfiddler.ui.resources import MAID_GUID_SLOT

class MaidInfoTab(UiTab):
    def __init__(self, ui, core, maid_mgr):
        UiTab.__init__(self, ui, core, maid_mgr)
        self.personalities = {}
        self.contracts = {}
        self.relations = {}
        self.seikeiken = {}
        self.job_classes = {}
        self.yotogi_classes = {}

    def update_ui(self):
        self.personalities.clear()
        self.contracts.clear()
        self.relations.clear()
        self.seikeiken.clear()
        self.job_classes.clear()
        self.yotogi_classes.clear()

        for i, personal in enumerate(self._game_data["personal_list"]):
            self.ui.personality_combo.addItem(personal["name"], personal["id"])
            self.personalities[personal["id"]] = i

        for i, contract_name in enumerate(self._game_data["contract"]):
            contract_id = self._game_data["contract"][contract_name]
            self.ui.contract_combo.addItem(contract_name, contract_id)
            self.contracts[contract_id] = i

        for i, relation_name in enumerate(self._game_data["relation"]):
            relation_id = self._game_data["relation"][relation_name]
            self.ui.relation_combo.addItem(relation_name, relation_id)
            self.relations[relation_id] = i

        for i, seik_name in enumerate(self._game_data["seikeiken"]):
            seik_id = self._game_data["seikeiken"][seik_name]
            self.ui.current_combo.addItem(seik_name, seik_id)
            self.ui.initial_combo.addItem(seik_name, seik_id)
            self.seikeiken[seik_id] = i

        for i, job_class in enumerate(self._game_data["job_class_list"]):
            self.ui.job_class_combo.addItem(job_class["name"], job_class["id"])
            self.job_classes[job_class["id"]] = i

        for i, yotogi_class in enumerate(self._game_data["yotogi_class_list"]):
            self.ui.yotogi_class_combo.addItem(yotogi_class["name"], yotogi_class["id"])
            self.yotogi_classes[yotogi_class["id"]] = i

        self.ui.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)

    def init_events(self, event_poller):
        self.ui.maid_list.currentItemChanged.connect(self.maid_selected)

    def maid_selected(self, n, p):
        if self.maid_mgr.selected_maid is None:
            return

        maid = self.maid_mgr.selected_maid
        print(f"MaidInfoTab: selected {maid['set_properties']['firstName']} {maid['set_properties']['lastName']}")
        self.ui.first_name_edit.setText(maid["set_properties"]["firstName"])
        self.ui.last_name_edit.setText(maid["set_properties"]["lastName"])
        self.ui.personality_combo.setCurrentIndex(self.personalities[maid["basic_properties"]["personal"]])
        self.ui.contract_combo.setCurrentIndex(self.contracts[maid["basic_properties"]["contract"]])
        self.ui.relation_combo.setCurrentIndex(self.relations[maid["enum_props"]["relation"]])
        self.ui.current_combo.setCurrentIndex(self.seikeiken[maid["basic_properties"]["cur_seikeiken"]])
        self.ui.initial_combo.setCurrentIndex(self.seikeiken[maid["basic_properties"]["cur_seikeiken"]])
        self.ui.job_class_combo.setCurrentIndex(self.job_classes[maid["basic_properties"]["current_job_class_id"]])
        self.ui.yotogi_class_combo.setCurrentIndex(self.yotogi_classes[maid["basic_properties"]["current_yotogi_class_id"]])
        self.ui.employment_day_box.setValue(maid["set_properties"]["employmentDay"])
        self.ui.maid_description_edit.setPlainText(maid["basic_properties"]["profile_comment"])
        self.ui.user_comment_text.setPlainText(maid["set_properties"]["freeComment"])


