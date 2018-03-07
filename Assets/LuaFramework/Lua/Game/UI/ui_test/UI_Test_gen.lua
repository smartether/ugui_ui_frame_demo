local UI_Test= UIManager.GetUIType('UI_Test','ui_test')
--  /** function PATH */
function UI_Test.PATH()
	return 'ui/UI_Test/UI_Test'
end


--  /** function BindView */
function UI_Test:BindView()
	self._node_trans = self.m_GameObject.transform
	self._image_recttrans = self.m_GameObject.transform:Find('widget_Panel/Image')
	self._image_image = self.m_GameObject.transform:Find('widget_Panel/Image'):GetComponent('UnityEngine.UI.Image')
	self._text_recttrans = self.m_GameObject.transform:Find('widget_Panel/Text')
	self._text_text = self.m_GameObject.transform:Find('widget_Panel/Text'):GetComponent('UnityEngine.UI.Text')
	self._button_image = self.m_GameObject.transform:Find('widget_Panel/Button'):GetComponent('UnityEngine.UI.Image')
	self._button_button = self.m_GameObject.transform:Find('widget_Panel/Button'):GetComponent('UnityEngine.UI.Button')
	self._btnName_text = self.m_GameObject.transform:Find('widget_Panel/Button/BtnName'):GetComponent('UnityEngine.UI.Text')
end

--  /** function UnBindView */
function UI_Test:UnBindView()
	self._node_trans = nil
	self._image_recttrans = nil
	self._image_image = nil
	self._text_recttrans = nil
	self._text_text = nil
	self._button_image = nil
	self._button_button = nil
	self._btnName_text = nil
	self.m_GameObject = nil
end

