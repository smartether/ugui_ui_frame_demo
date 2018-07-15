local UI_Test= UIManager.GetUIType('UI_Test','ui_test')
--  /** function PATH */
function UI_Test.PATH()
	return 'ui/UI_Test/UI_Test'
end


--  /** function BindView */
function UI_Test:BindView()
	self._node_trans = self.m_GameObject.transform
	self._pic1_image = self.m_GameObject.transform:Find('pic1'):GetComponent('UnityEngine.UI.Image')
	self._im1424_recttrans = self.m_GameObject.transform:Find('pic1/Im1424')
	self._im1424_image = self.m_GameObject.transform:Find('pic1/Im1424'):GetComponent('UnityEngine.UI.Image')
	self._lab_recttrans = self.m_GameObject.transform:Find('pic1/lab')
	self._lab_text = self.m_GameObject.transform:Find('pic1/lab'):GetComponent('UnityEngine.UI.Text')
	self._ewyweywey_recttrans = self.m_GameObject.transform:Find('pic1/ewyweywey')
	self._ewyweywey_image = self.m_GameObject.transform:Find('pic1/ewyweywey'):GetComponent('UnityEngine.UI.Image')
	self._ewyweywey_button = self.m_GameObject.transform:Find('pic1/ewyweywey'):GetComponent('UnityEngine.UI.Button')
	self._btn1_button = self.m_GameObject.transform:Find('pic1/btn1'):GetComponent('UnityEngine.UI.Button')
	self._btnName_recttrans = self.m_GameObject.transform:Find('pic1/ewyweywey/BtnName')
	self._btnName_text = self.m_GameObject.transform:Find('pic1/ewyweywey/BtnName'):GetComponent('UnityEngine.UI.Text')
end

--  /** function UnBindView */
function UI_Test:UnBindView()
	self._node_trans = nil
	self._pic1_image = nil
	self._im1424_recttrans = nil
	self._im1424_image = nil
	self._lab_recttrans = nil
	self._lab_text = nil
	self._ewyweywey_recttrans = nil
	self._ewyweywey_image = nil
	self._ewyweywey_button = nil
	self._btn1_button = nil
	self._btnName_recttrans = nil
	self._btnName_text = nil
	self.m_GameObject = nil
end

