using Chat.Server.Data.Entity;

namespace Chat.Server.GraphQL;

public class Subscription
{
    [Subscribe]
    public DirectMessage DirectMessageAdded([EventMessage] DirectMessage message) => message;

    [Subscribe]
    public SpaceMessage SpaceMessageAdded([EventMessage] SpaceMessage message) => message;
}