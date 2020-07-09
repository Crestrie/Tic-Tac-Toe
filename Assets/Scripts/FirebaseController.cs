using System.Linq;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FirebaseController : MonoBehaviour
{
    private static FirebaseApp BaseApp;
    private static DatabaseReference BaseReference;

    private static string PrettyPlayerText = "Player";
    private static string PrettyRoomText = "Room";
    private static int RoomCapacity = 2;

    public static string MyName = "";
    public static string MyRoom = "";
    
    private static int OnChangeCallCount = 0;
    
    /*
    
    private static readonly string[] Letter =
    {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "1", "2", "3", "4", "5", "6", "7", "8", "9"
    };
    
    */
    
    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            if (task.Result == DependencyStatus.Available) 
            {
                BaseApp = FirebaseApp.DefaultInstance;
                BaseApp.SetEditorDatabaseUrl("https://trillare-tic-tac-toe.firebaseio.com/");
                BaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
        });
    }

    private static string Create(DataSnapshot Snapshot, string PrettyText)
    {
        int NextNumber = 0;
                
        for (int i = 0; i < Snapshot.Children.Count(); i++)
        {
            int Count = 0;
                    
            foreach (DataSnapshot Child in Snapshot.Children)
            {
                int Number = int.Parse(Child.Key.Substring(PrettyText.Length));

                if (i + 1 == Number)
                {
                    break;
                }

                if (Count == Snapshot.Children.Count() - 1)
                {
                    NextNumber = i + 1;

                    if (NextNumber < 10)
                    {
                        return PrettyText + "0" + NextNumber;
                    }
                    else
                    {
                        return PrettyText + NextNumber;
                    }
                }
                        
                Count++;
            }
        }

        NextNumber = Snapshot.Children.Count() + 1;
        
        if (NextNumber < 10)
        {
            return PrettyText + "0" + NextNumber;
        }
        else
        {
            return PrettyText + NextNumber;
        }
    }

    private static string SearchReference(DataSnapshot Snapshot, string Name)
    {
        string ReferenceKey = "";
        
        if (Snapshot.Children.Any())
        {
            foreach (DataSnapshot Child in Snapshot.Children)
            {
                if (Child.Key != Name)
                {
                    ReferenceKey = SearchReference(Child, Name);

                    if (ReferenceKey != "")
                    {
                        break;
                    }
                }
                else
                {
                    ReferenceKey = Snapshot.Key;
                    
                    break;
                }
            }
        }

        return ReferenceKey;
    }

    private static void OnChange(object Sender, ValueChangedEventArgs Argument)
    {
        if (Argument.DatabaseError != null)
        {
            return;
        }

        if (Argument.Snapshot.Value != null)
        {
            return;
        }
        else
        {
            if (OnChangeCallCount > 0)
            {
                BaseReference.Child("Lobby").Child(MyName).ValueChanged -= OnChange;
                
                int OnMakingRoomCallCount = 0;
                
                void OnMakingRoom(object RoomSender, ValueChangedEventArgs RoomArgument)
                {
                    if (OnMakingRoomCallCount > 0)
                    {
                        DataSnapshot ActiveRoom = RoomArgument.Snapshot;
                        MyRoom = SearchReference(ActiveRoom, MyName);
                        
                        Debug.Log("Player: " + MyName + ", Room: " + MyRoom);

                        if (MyRoom != "")
                        {
                            BaseReference.Child("ActiveRoom").ValueChanged -= OnMakingRoom;
                        }
                    }
                    else
                    {
                        OnMakingRoomCallCount++;
                    }
                }

                if (OnMakingRoomCallCount < 1)
                {
                    BaseReference.Child("ActiveRoom").ValueChanged += OnMakingRoom;
                }
            }
            else
            {
                OnChangeCallCount++;
            }
        }
    }

    public static void ConnectToLobby()
    {
        BaseReference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return;
            }

            DataSnapshot ActivePlayer = task.Result.Child("ActivePlayer");
            DataSnapshot ActiveRoom = task.Result.Child("ActiveRoom");
            DataSnapshot Lobby = task.Result.Child("Lobby");

            if (ActivePlayer.Children.Any())
            {
                MyName = Create(ActivePlayer, PrettyPlayerText);
                BaseReference.Child("ActivePlayer").Child(MyName).SetValueAsync(0);

                if (Lobby.Children.Count() + 1 >= RoomCapacity)
                {
                    if (ActiveRoom.Children.Any())
                    {
                        MyRoom = Create(ActiveRoom, PrettyRoomText);
                    }
                    else
                    {
                        MyRoom = PrettyRoomText + "01";
                    }
                    
                    BaseReference.Child("ActiveRoom").Child(MyRoom).Child(MyName).SetValueAsync(0);
                    
                    Debug.Log("Player: " + MyName + ", Room: " + MyRoom);
                    
                    for (int i = 1; i < RoomCapacity; i++)
                    {
                        string LastPlayer = Lobby.Children.Last().Key;
                        
                        BaseReference.Child("Lobby").Child(LastPlayer).SetValueAsync(null);
                        BaseReference.Child("ActiveRoom").Child(MyRoom).Child(LastPlayer).SetValueAsync(0);
                    }
                    
                    UiConnect.ScreenOut();
                }
                else
                {
                    BaseReference.Child("Lobby").Child(MyName).SetValueAsync(0);
                    BaseReference.Child("Lobby").Child(MyName).ValueChanged += OnChange;
                }
            }
            else
            {
                MyName = PrettyPlayerText + "01";
                
                BaseReference.Child("ActivePlayer").Child(MyName).SetValueAsync(0);
                BaseReference.Child("Lobby").Child(MyName).SetValueAsync(0);
                BaseReference.Child("Lobby").Child(MyName).ValueChanged += OnChange;
            }
        });
    }
}