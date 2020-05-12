# Unity-KinectDepthTrigger
**說明**<br>
* 使用`Kinect`達成`深度`的感測

**主要功能**<br>
* 按下空白鍵後開始偵測
* 透過參數可以調整要偵測的距離以及範圍
* 使用`Median Filtering`進行雜訊濾波
* 提供API讓rect範圍內偵測點數量超過某數(預設為5)時判斷為觸發

**DEMO**<br>
* 沒有進行濾波的圖像
![GITHUB]( https://github.com/Gougon-Side-Project/Unity-KinectDepthTrigger/blob/master/No%20filter.png "沒有進行濾波的圖像")
* 尚未進到感測範圍
![GITHUB]( https://github.com/Gougon-Side-Project/Unity-KinectDepthTrigger/blob/master/demo-0.png "沒有進行濾波的圖像")
* 手部進到感測範圍
![GITHUB]( https://github.com/Gougon-Side-Project/Unity-KinectDepthTrigger/blob/master/demo-1.png "沒有進行濾波的圖像")
* 身體進到感測範圍
![GITHUB]( https://github.com/Gougon-Side-Project/Unity-KinectDepthTrigger/blob/master/demo-2.png "沒有進行濾波的圖像")
