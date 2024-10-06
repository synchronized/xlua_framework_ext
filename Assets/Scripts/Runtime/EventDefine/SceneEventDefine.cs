﻿using UniFramework.Event;

public class SceneEventDefine
{
    public class ChangeToLoginScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToLoginScene();
            UniEvent.SendMessage(msg);
        }
    }

    public class ChangeToMainScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToMainScene();
            UniEvent.SendMessage(msg);
        }
    }

    public class ChangeToBattleScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToBattleScene();
            UniEvent.SendMessage(msg);
        }
    }
}
