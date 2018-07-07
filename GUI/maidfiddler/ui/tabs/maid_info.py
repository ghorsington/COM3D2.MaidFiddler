from PyQt5.QtCore import Qt, QObject
from .ui_tab import UiTab
from maidfiddler.ui.qt_elements import TextElement, ComboElement, NumberElement, PlainTextElement
from zerorpc import RemoteError


class MaidInfoTab(UiTab):
    def __init__(self, ui, core, maid_mgr):
        UiTab.__init__(self, ui, core, maid_mgr)
        self.personalities = {}
        self.contracts = {}
        self.relations = {}
        self.seikeiken = {}
        self.job_classes = {}
        self.yotogi_classes = {}

        self.properties = {
            "firstName": TextElement(self.ui.first_name_edit),
            "lastName": TextElement(self.ui.last_name_edit),
            "personal": ComboElement(self.ui.personality_combo, self.personalities),
            "contract": ComboElement(self.ui.contract_combo, self.contracts),
            "relation": ComboElement(self.ui.relation_combo, self.relations),
            "cur_seikeiken": ComboElement(self.ui.current_combo, self.seikeiken),
            "init_seikeiken": ComboElement(self.ui.initial_combo, self.seikeiken),
            "current_job_class_id": ComboElement(self.ui.job_class_combo, self.job_classes),
            "current_yotogi_class_id": ComboElement(self.ui.yotogi_class_combo, self.yotogi_classes),
            "employmentDay": NumberElement(self.ui.employment_day_box),
            "profile_comment": PlainTextElement(self.ui.maid_description_edit),
            "freeComment": PlainTextElement(self.ui.user_comment_text)
        }

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
            self.ui.yotogi_class_combo.addItem(
                yotogi_class["name"], yotogi_class["id"])
            self.yotogi_classes[yotogi_class["id"]] = i

        self.ui.yotogi_class_combo.view().setVerticalScrollBarPolicy(Qt.ScrollBarAsNeeded)

    def init_events(self, event_poller):
        self.ui.maid_list.currentItemChanged.connect(self.maid_selected)

        self.ui.first_name_edit.editingFinished.connect(
            self.commit_prop_changes("firstName"))

        self.ui.last_name_edit.editingFinished.connect(
            self.commit_prop_changes("lastName"))

        self.ui.relation_combo.currentIndexChanged.connect(
            self.commit_prop_changes("relation"))

        self.ui.employment_day_box.valueChanged.connect(
            self.commit_prop_changes("employmentDay"))

        # self.ui.user_comment_text.currentIndexChanged.connect(
        #     self.commit_prop_changes("freeComment"))

        self.ui.personality_combo.currentIndexChanged.connect(lambda: self.core.SetPersonal(
            self.maid_mgr.selected_maid["guid"], self.properties["personal"].value()))

        self.ui.contract_combo.currentIndexChanged.connect(lambda: self.core.SetContract(
            self.maid_mgr.selected_maid["guid"], self.properties["contract"].value()))

        self.ui.current_combo.currentIndexChanged.connect(lambda: self.core.SetCurSeikeiken(
            self.maid_mgr.selected_maid["guid"], self.properties["cur_seikeiken"].value()))

        self.ui.initial_combo.currentIndexChanged.connect(lambda: self.core.SetInitSeikeiken(
            self.maid_mgr.selected_maid["guid"], self.properties["init_seikeiken"].value()))

        self.ui.job_class_combo.currentIndexChanged.connect(lambda: self.core.SetCurrentJobClass(
            self.maid_mgr.selected_maid["guid"], self.properties["current_job_class_id"].value()))

        self.ui.yotogi_class_combo.currentIndexChanged.connect(lambda: self.core.SetCurrentYotogiClass(
            self.maid_mgr.selected_maid["guid"], self.properties["current_yotogi_class_id"].value()))

        event_poller.on("maid_prop_changed", self.prop_changed)

    def commit_prop_changes(self, prop):
        def handler():
            print(f"Commiting changes for {prop}")
            try:
                self.core.SetMaidProperty(
                    self.maid_mgr.selected_maid["guid"], prop, self.properties[prop].value())
            except Exception as e:
                print(f"{e}")
        return handler

    def prop_changed(self, args):
        print(f"Property changed: {args['property_name']}")
        if args["property_name"] not in self.properties:
            return

        self.properties[args["property_name"]].set_value(args["value"])

    def maid_selected(self, n, p):
        if self.maid_mgr.selected_maid is None:
            return

        maid = self.maid_mgr.selected_maid

        for name, element in self.properties.items():
            element.set_value(maid["properties"][name])
