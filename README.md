# KomeTube
讀取Youtube Live聊天室留言  
提供投票統計、文字猜謎等互動功能  

![](https://github.com/dghkd/KomeTube/raw/master/preview1.png)

操作方法:  
1. 左上角輸入直播影片網址  
2. 點擊開始  
開始後便會持續讀取聊天室留言並顯示於下方  
若停止後重新開始時，則只會從當下聊天室存在留言開始擷取  


## 程式方法  
1. 由輸入的直播影片網址中取得Video ID  
2. 透過Video ID取得聊天室HTML內容 (https://www.youtube.com/live_chat?v={vid}&is_popout=1)  
3. 從HTML內容中解析出continuation參數 (window["ytInitialData"]的值)  
4. 利用continuation參數取得聊天室留言以及下一次的continuation參數 (https://www.youtube.com/live_chat/get_live_chat?continuation={continuation.Continuation}&pbj=1)  
5. 循環利用continuation參數取得新留言  
