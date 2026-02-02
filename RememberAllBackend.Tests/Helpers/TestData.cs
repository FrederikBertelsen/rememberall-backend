using System.Data.Common;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Entities;

namespace RememberAllBackend.Tests.Helpers;

public static class TestData
{
    private const string DefaultName = "Test User";
    private const string DefaultEmail = "user@example.com";
    private const string DefaultPassword = "TestPass1234!@#";
    private const string DefaultListName = "Test List";
    private const string DefaultItemText = "Test Item";

    #region Builder Factory Methods

    public static UserBuilder User() => new();

    public static TodoListBuilder TodoList() => new();

    public static TodoItemBuilder TodoItem() => new();

    public static ScenarioBuilder Scenario() => new();

    #endregion

    #region DTO Factory Methods

    public static CreateUserDto CreateUserDto(
        string? name = null,
        string? email = null,
        string? password = null)
    {
        return new CreateUserDto(
            name ?? DefaultName,
            email ?? DefaultEmail,
            password ?? DefaultPassword);
    }

    public static LoginDto LoginDto(
        string? email = null,
        string? password = null)
    {
        return new LoginDto(
            email ?? DefaultEmail,
            password ?? DefaultPassword);
    }

    public static CreateTodoListDto CreateTodoListDto(string? name = null)
    {
        return new CreateTodoListDto(name ?? DefaultListName);
    }

    public static TodoListDto TodoListDto(
        Guid? id = null,
        Guid? ownerId = null,
        string? name = null,
        ICollection<TodoItemDto>? items = null)
    {
        return new TodoListDto(
            id ?? Guid.NewGuid(),
            ownerId ?? Guid.NewGuid(),
            name ?? DefaultListName,
            items ?? []);
    }

    public static CreateTodoItemDto CreateTodoItemDto(
        Guid? todoListId = null,
        string? text = null)
    {
        return new CreateTodoItemDto(
            todoListId ?? Guid.NewGuid(),
            text ?? DefaultItemText);
    }

    #endregion

    #region Builders

    public class UserBuilder
    {
        private Guid? _id;
        private string _name = DefaultName;
        private string _email = DefaultEmail;
        private readonly List<TodoList> _lists = [];

        public UserBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public UserBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public UserBuilder WithList(TodoList list)
        {
            _lists.Add(list);
            return this;
        }

        public UserBuilder WithLists(IEnumerable<TodoList> lists)
        {
            _lists.AddRange(lists);
            return this;
        }

        public UserBuilder WithLists(int count, string? namePrefix = null)
        {
            namePrefix ??= DefaultListName;

            for (int i = 0; i < count; i++)
            {
                var list = TodoList()
                    .WithName($"{namePrefix} {i}")
                    .Build();

                _lists.Add(list);
            }

            return this;
        }

        public User Build()
        {
            var user = new User
            {
                Id = _id ?? Guid.NewGuid(),
                Name = _name,
                Email = _email
            };

            foreach (var list in _lists)
            {
                list.OwnerId = user.Id;
                user.Lists.Add(list);
            }

            return user;
        }
    }

    public class TodoListBuilder
    {
        private Guid? _id;
        private Guid? _ownerId;
        private string _name = DefaultListName;
        private readonly List<TodoItem> _items = [];

        public TodoListBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public TodoListBuilder WithOwnerId(Guid ownerId)
        {
            _ownerId = ownerId;
            return this;
        }

        public TodoListBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public TodoListBuilder WithItem(TodoItem item)
        {
            _items.Add(item);
            return this;
        }

        public TodoListBuilder WithItems(IEnumerable<TodoItem> items)
        {
            _items.AddRange(items);

            return this;
        }

        public TodoListBuilder WithItems(int count, string? textPrefix = null, bool allCompleted = false)
        {
            textPrefix ??= DefaultItemText;

            for (int i = 0; i < count; i++)
            {
                var item = TodoItem()
                    .WithText($"{textPrefix} {i}");

                if (allCompleted)
                    item.AsCompleted();

                _items.Add(item.Build());
            }
            return this;
        }

        public TodoList Build()
        {
            if (_ownerId is null)
                throw new InvalidOperationException("OwnerId must be set before building a TodoList.");

            var list = new TodoList
            {
                Id = _id ?? Guid.NewGuid(),
                OwnerId = _ownerId.Value,
                Name = _name
            };

            foreach (var item in _items)
            {
                item.TodoListId = list.Id;
                list.Items.Add(item);
            }

            return list;
        }
    }

    public class TodoItemBuilder
    {
        private Guid? _id;
        private Guid? _todoListId;
        private string _text = DefaultItemText;
        private bool _isCompleted;

        public TodoItemBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public TodoItemBuilder WithTodoListId(Guid todoListId)
        {
            _todoListId = todoListId;
            return this;
        }

        public TodoItemBuilder WithText(string text)
        {
            _text = text;
            return this;
        }

        public TodoItemBuilder AsCompleted()
        {
            _isCompleted = true;
            return this;
        }

        public TodoItemBuilder AsIncomplete()
        {
            _isCompleted = false;
            return this;
        }

        public TodoItem Build()
        {
            // if (_todoListId is null)
            //     throw new InvalidOperationException("TodoListId must be set before building a TodoItem.");

            var item = new TodoItem
            {
                Id = _id ?? Guid.NewGuid(),
                TodoListId = _todoListId ?? new Guid(),
                Text = _text
            };

            if (_isCompleted)
                item.MarkAsComplete();

            return item;
        }
    }

    public class ScenarioBuilder
    {
        private string _userName = DefaultName;
        private string _userEmail = DefaultEmail;
        private string _listName = DefaultListName;
        private int _itemCount = 3;
        private string? _itemTextPrefix = null;
        private readonly List<TodoList> _additionalLists = [];

        public ScenarioBuilder WithUser(string name, string? email = null)
        {
            _userName = name;
            _userEmail = email ?? $"{name.ToLower().Replace(" ", "")}@example.com";
            return this;
        }

        public ScenarioBuilder WithList(string name)
        {
            _listName = name;
            return this;
        }

        public ScenarioBuilder WithItems(int count, string? textPrefix = null)
        {
            _itemCount = count;
            _itemTextPrefix = textPrefix;
            return this;
        }

        public ScenarioBuilder WithAdditionalList(TodoList list)
        {
            _additionalLists.Add(list);
            return this;
        }

        public ScenarioBuilder WithAdditionalLists(int count, string? namePrefix = null)
        {
            namePrefix ??= DefaultListName;

            for (int i = 0; i < count; i++)
            {
                var list = TodoList()
                    .WithName($"{namePrefix} {i}")
                    .Build();
                _additionalLists.Add(list);
            }

            return this;
        }

        public (User User, TodoList List, List<TodoItem> Items) Build()
        {
            var user = User()
                .WithName(_userName)
                .WithEmail(_userEmail)
                .Build();

            var list = TodoList()
                .WithOwnerId(user.Id)
                .WithName(_listName)
                .WithItems(_itemCount, _itemTextPrefix)
                .Build();

            user.Lists.Add(list);

            foreach (var additionalList in _additionalLists)
            {
                additionalList.OwnerId = user.Id;
                user.Lists.Add(additionalList);
            }

            return (user, list, list.Items.ToList());
        }

        public static List<(User User, List<TodoList> Lists)> BuildMultiUser(
            int userCount = 2,
            int listsPerUser = 2,
            int itemsPerList = 3)
        {
            var scenarios = new List<(User, List<TodoList>)>();

            for (int i = 0; i < userCount; i++)
            {
                var user = User()
                    .WithName($"{DefaultName} {i}")
                    .WithEmail($"{DefaultName.ToLower().Replace(" ", "")}{i}@example.com")
                    .Build();

                var lists = new List<TodoList>();
                for (int j = 0; j < listsPerUser; j++)
                {
                    var list = TodoList()
                        .WithOwnerId(user.Id)
                        .WithName($"{DefaultListName} U{i}-{j}")
                        .WithItems(itemsPerList)
                        .Build();

                    user.Lists.Add(list);
                    lists.Add(list);
                }

                scenarios.Add((user, lists));
            }

            return scenarios;
        }
    }

    #endregion
}

