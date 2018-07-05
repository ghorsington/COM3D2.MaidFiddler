from PyQt5.QtWidgets import QListWidgetItem
from PyQt5.QtCore import Qt
from .ui_tab import UiTab


class FeaturePropensityTab(UiTab):

    def update_ui(self):
        # Feature list

        for feature in self.game_data["feature_list"]:
            item = QListWidgetItem(feature["name"])
            item.setData(Qt.UserRole, feature["id"])
            item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
            item.setCheckState(Qt.Unchecked)
            self.ui.feature_list.addItem(item)

        # Propensity list

        for propensity in self.game_data["propensity_list"]:
            item = QListWidgetItem(propensity["name"])
            item.setData(Qt.UserRole, propensity["id"])
            item.setFlags(item.flags() | Qt.ItemIsUserCheckable)
            item.setCheckState(Qt.Unchecked)
            self.ui.propensity_list.addItem(item)
