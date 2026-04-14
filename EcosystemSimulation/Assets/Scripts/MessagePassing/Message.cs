using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MsgType
{
    Food_Request,
    Food_Deny,
    Food_Accept,
    Crossover_Request,
    Crossover_Deny,
    Crossover_Accept
}
public class Message
{
    MsgType msgType;
    Entity sender;

    public Message(MsgType msgType1, Entity sender1)
    {
        msgType = msgType1;
        sender = sender1;
    }
    public MsgType GetMsgType()
    {
        return msgType;
    }
    public Entity GetSender()
    {
        return sender;
    }
    public void SetMsgType(MsgType newMsgType)
    {
        msgType = newMsgType;
    }
}
