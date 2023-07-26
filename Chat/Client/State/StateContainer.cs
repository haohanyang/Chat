
using Chat.Client.Models;
using Chat.Shared.Dto;

namespace Chat.Client.State;

public class StateContainer
{
    private LinkedList<Contact>? _contacts;
    public int? SelectedChatId { get; set; }
    public ChatType? SelectedChatType { get; set; }

    private string? _contactError { get; set; }

    public LinkedList<Contact>? Contacts
    {
        get => _contacts;
        set
        {
            _contacts = value;
            NavOnChange?.Invoke();
        }
    }

    public string? ContactError
    {
        get => _contactError;
        set
        {
            _contactError = value;
            NavOnChange?.Invoke();
        }
    }

    public void AddContact(Contact contact)
    {
        if (Contacts != null)
        {
            if (Contacts.FirstOrDefault(c => c.Id == contact.Id && c.Type == contact.Type) == null)
            {
                Contacts.AddFirst(contact);
                NavOnChange?.Invoke();
            }
        }
    }

    public void UpdateMessages(ChatType type, int chatId, List<MessageDto> messages)
    {
        if (Contacts != null)
        {
            var contact = Contacts.FirstOrDefault(c => c.Id == chatId && c.Type == type);
            if (contact != null)
            {
                contact.Messages = messages;
            }
        }
    }

    public void UpdateMessage(MessageDto message)
    {
        if (Contacts != null)
        {
            var contact = Contacts?.FirstOrDefault(c => c.Id == message.ChatId && c.Type == message.Type);
            if (contact != null)
            {
                contact.Messages?.Add(message);
                bool renderNav = false;

                if (Contacts!.First() != contact)
                {
                    Contacts!.Remove(contact);
                    Contacts.AddFirst(contact);
                    renderNav = true;
                }
                if (message.ChatId != SelectedChatId || message.Type != SelectedChatType)
                {
                    contact.UnreadMessageCount++;
                    renderNav = true;
                }
                else
                {
                    MainOnChange?.Invoke();
                }

                if (renderNav)
                {
                    NavOnChange?.Invoke();
                }
            }
            else
            {
                Contacts!.AddFirst(new Contact
                {
                    Id = message.ChatId,
                    Type = message.Type,
                    Messages = new List<MessageDto> { message },
                    UnreadMessageCount = 1,
                    Name = message.Type == ChatType.Direct ? message.Author.Name : message.Name
                });
            }
        }

    }

    public List<MessageDto>? GetMessages(ChatType type, int chatId)
    {
        if (Contacts != null)
        {
            var contact = Contacts.FirstOrDefault(c => c.Id == chatId && c.Type == type);
            if (contact != null)
            {
                contact.UnreadMessageCount = 0;
                NavOnChange?.Invoke();
                return contact.Messages;
            }
        }
        throw new ArgumentException("Contact not found");
    }

    public event Action? NavOnChange;
    public event Action? MainOnChange;
}
