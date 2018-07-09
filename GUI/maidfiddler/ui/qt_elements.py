class UiElement(object):
    def __init__(self, qt_element):
        self.qt_element = qt_element

    def value(self):
        raise NotImplementedError()

    def set_value(self, val):
        raise NotImplementedError()

    def connect(self, edit_func):
        pass

class TextElement(UiElement):
    def value(self):
        return self.qt_element.text()

    def set_value(self, val):
        self.qt_element.setText(val)

class PlainTextElement(UiElement):
    def value(self):
        return self.qt_element.plainText()

    def set_value(self, val):
        self.qt_element.setPlainText(val)

    def connect(self, edit_func):
        self.qt_element.editingFinished.connect(edit_func)

class NumberElement(UiElement):
    def __init__(self, qt_element):
        UiElement.__init__(self, qt_element)

        self.qt_element.setMaximum(2**31 - 1)

    def value(self):
        return self.qt_element.value()

    def set_value(self, val):
        self.qt_element.blockSignals(True)
        self.qt_element.setValue(val)
        self.qt_element.blockSignals(False)

    def connect(self, edit_func):
        self.qt_element.valueChanged.connect(edit_func)

class ComboElement(UiElement):
    def __init__(self, qt_element, value_to_index_map):
        UiElement.__init__(self, qt_element)
        self.value_to_index_map = value_to_index_map

    def value(self):
        return self.qt_element.currentData()

    def set_value(self, val):
        self.qt_element.blockSignals(True)
        self.qt_element.setCurrentIndex(self.value_to_index_map[val])
        self.qt_element.blockSignals(False)