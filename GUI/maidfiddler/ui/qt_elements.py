class UiElement(object):
    def __init__(self, qt_element):
        self.qt_element = qt_element

    def value(self):
        raise NotImplementedError()

    def set_value(self, val):
        raise NotImplementedError()

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

class NumberElement(UiElement):
    def value(self):
        return self.qt_element.value()

    def set_value(self, val):
        self.qt_element.setValue(val)

class ComboElement(UiElement):
    def __init__(self, qt_element, value_to_index_map):
        UiElement.__init__(self, qt_element)
        self.value_to_index_map = value_to_index_map

    def value(self):
        return self.qt_element.currentData()

    def set_value(self, val):
        self.qt_element.setCurrentIndex(self.value_to_index_map[val])