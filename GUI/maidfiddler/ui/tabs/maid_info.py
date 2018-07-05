from PyQt5.QtCore import Qt
from .ui_tab import UiTab


class MaidInfoTab(UiTab):
    def update_ui(self):
        for personal in self._game_data["personal_list"]:
            self.ui.personality_combo.addItem(personal["name"], personal["id"])

        for (contract_name, contract_id) in self._game_data["contract"].items():
            self.ui.contract_combo.addItem(contract_name, contract_id)

        for (relation_name, relation_id) in self._game_data["relation"].items():
            self.ui.relation_combo.addItem(relation_name, relation_id)

        for (seik_name, seik_id) in self._game_data["seikeiken"].items():
            self.ui.current_combo.addItem(seik_name, seik_id)
            self.ui.initial_combo.addItem(seik_name, seik_id)

        for job_class in self._game_data["job_class_list"]:
            self.ui.job_class_combo.addItem(job_class["name"], job_class["id"])

        for yotogi_class in self._game_data["yotogi_class_list"]:
            self.ui.yotogi_class_combo.addItem(
                yotogi_class["name"], yotogi_class["id"])

        self.ui.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)
