using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LucidSightTools;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Colyseus
{
    /// <summary>
    ///     Colyseus.Client
    /// </summary>
    /// <remarks>
    ///     Provides integration between Colyseus Game Server through WebSocket protocol (
    ///     <see href="http://tools.ietf.org/html/rfc6455">RFC 6455</see>).
    /// </remarks>
    public class ColyseusClient
    {
        /// <summary>
        ///     Delegate function for when the <see cref="ColyseusClient" /> successfully connects to the
        ///     <see cref="ColyseusRoom{T}" />.
        /// </summary>
        public delegate void ColyseusAddRoomEventHandler(IColyseusRoom room);

        /// <summary>
        ///     Reference to the client's <see cref="UriBuilder" />
        /// </summary>
        public UriBuilder Endpoint;

		/// <summary>
		/// The <see cref="ColyseusSettings"/> currently assigned to this client object
		/// </summary>
		private ColyseusSettings _colyseusSettings;

		/// <summary>
		///     Occurs when the <see cref="ColyseusClient" /> successfully connects to the <see cref="ColyseusRoom{T}" />.
		/// </summary>
		public static event ColyseusAddRoomEventHandler onAddRoom;

		/// <summary>
		/// The getter for the <see cref="ColyseusSettings"/> currently assigned to this client object
		/// </summary>
		public ColyseusSettings Settings
        {
	        get
	        {
		        return _colyseusSettings;
	        }

	        private set
	        {
		        _colyseusSettings = value;

		        // Instantiate our ColyseusRequest object with the settings object
				colyseusRequest = new ColyseusRequest(_colyseusSettings);
	        }
        }

        /// <summary>
		/// Object to perform <see cref="UnityEngine.Networking.UnityWebRequest"/>s to the server.
		/// </summary>
        public ColyseusRequest colyseusRequest;

		/// <summary>
		///     Initializes a new instance of the <see cref="ColyseusClient" /> class with
		///     the specified Colyseus Game Server endpoint.
		/// </summary>
		/// <param name="endpoint">
		///     A <see cref="string" /> that represents the WebSocket URL to connect.
		/// </param>
		public ColyseusClient(string endpoint)
		{
            Debug.Log("Endpoint? "+endpoint);
			Endpoint = new UriBuilder(endpoint);
            Debug.Log("Endpoint succeed? " + Endpoint.Host);
            Debug.Log(Endpoint);
			// Create ColyseusSettings object to pass to the ColyseusRequest object
			ColyseusSettings settings = ScriptableObject.CreateInstance<ColyseusSettings>();
			settings.colyseusServerAddress = $"{Endpoint.Host}{Endpoint.Path}";
			settings.colyseusServerPort = Endpoint.Port.ToString();
			settings.useSecureProtocol = string.Equals(Endpoint.Scheme, "wss") || string.Equals(Endpoint.Scheme, "https");

			Settings = settings;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColyseusClient"/> class with
		/// the specified Colyseus Settings object.
		/// </summary>
		/// <param name="settings">The settings you wish to use</param>
		/// <param name="useWebSocketEndpoint">Determines whether the connection endpoint should use either web socket or http protocols.</param>
		public ColyseusClient(ColyseusSettings settings, bool useWebSocketEndpoint)
        {
	        SetSettings(settings, useWebSocketEndpoint);
        }

		public void SetSettings(ColyseusSettings settings, bool useWebSocketEndpoint)
		{
			Endpoint = new UriBuilder(useWebSocketEndpoint ? settings.WebSocketEndpoint : settings.WebRequestEndpoint);

			Settings = settings;
		}

        /// <summary>
        ///     Join or Create a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon creation/joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we create/join the room</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to join or create</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<T>> JoinOrCreate<T>(string roomName, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<T>("joinOrCreate", roomName, options, headers);
        }

        /// <summary>
        ///     Create a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon creation</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we create the room</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to create</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<T>> Create<T>(string roomName, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<T>("create", roomName, options, headers);
        }

        /// <summary>
        ///     Join a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we join the room</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to join</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<T>> Join<T>(string roomName, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<T>("join", roomName, options, headers);
        }

        /// <summary>
        ///     Join a <see cref="ColyseusRoom{T}" /> by ID
        /// </summary>
        /// <param name="roomId">ID of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we join the room</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to join</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<T>> JoinById<T>(string roomId, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<T>("joinById", roomId, options, headers);
        }

        /// <summary>
        ///     Reconnect to a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomId">ID of the room</param>
        /// <param name="sessionId">Previously connected sessionId</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we reconnect to the room</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to reconnect with</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<T>> Reconnect<T>(string roomId, string sessionId,
            Dictionary<string, string> headers = null)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("sessionId", sessionId);
            return await CreateMatchMakeRequest<T>("joinById", roomId, options, headers);
        }

        //
        // FossilDelta/None serializer versions for joining the state
        //
        /// <summary>
        ///     Join or Create a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon creation/joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we create/join the room</param>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<dynamic>> JoinOrCreate(string roomName,
            Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<dynamic>("joinOrCreate", roomName, options, headers);
        }

        /// <summary>
        ///     Create a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon creation</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we create the room</param>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<dynamic>> Create(string roomName, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<dynamic>("create", roomName, options, headers);
        }

        /// <summary>
        ///     Join a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we join the room</param>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<dynamic>> Join(string roomName, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<dynamic>("join", roomName, options, headers);
        }

        /// <summary>
        ///     Join a <see cref="ColyseusRoom{T}" /> by ID
        /// </summary>
        /// <param name="roomId">ID of the room</param>
        /// <param name="options">Dictionary of options to pass to the room upon joining</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we join the room</param>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<dynamic>> JoinById(string roomId, Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            return await CreateMatchMakeRequest<dynamic>("joinById", roomId, options, headers);
        }

        /// <summary>
        ///     Reconnect to a <see cref="ColyseusRoom{T}" />
        /// </summary>
        /// <param name="roomId">ID of the room</param>
        /// <param name="sessionId">Previously connected sessionId</param>
        /// <param name="headers">Dictionary of headers to pass to the server when we reconnect to the room</param>
        /// <returns><see cref="ColyseusRoom{T}" /> via async task</returns>
        public async Task<ColyseusRoom<dynamic>> Reconnect(string roomId, string sessionId,
            Dictionary<string, string> headers = null)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("sessionId", sessionId);
            return await CreateMatchMakeRequest<dynamic>("joinById", roomId, options, headers);
        }

        /// <summary>
        ///     Get all available rooms
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="headers">Dictionary of headers to pass to the server</param>
        /// <returns><see cref="ColyseusRoomAvailable" /> array via async task</returns>
        public async Task<ColyseusRoomAvailable[]> GetAvailableRooms(string roomName = "",
            Dictionary<string, string> headers = null)
        {
            return await GetAvailableRooms<ColyseusRoomAvailable>(roomName, headers);
        }

        /// <summary>
        ///     Get all available rooms of type <typeparamref name="T" />
        /// </summary>
        /// <param name="roomName">Name of the room</param>
        /// <param name="headers">Dictionary of headers to pass to the server</param>
        /// <returns><see cref="CSACSARoomAvailableCollection{T}" /> array via async task</returns>
        public async Task<T[]> GetAvailableRooms<T>(string roomName = "", Dictionary<string, string> headers = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            string json =
                await colyseusRequest.Request("GET", $"matchmake/{roomName}", null,
                    headers); //req.downloadHandler.text;
            if (json.StartsWith("[", StringComparison.CurrentCulture))
            {
                json = "{\"rooms\":" + json + "}";
            }

            CSARoomAvailableCollection<T> response = JsonUtility.FromJson<CSARoomAvailableCollection<T>>(json);
            return response.rooms;
        }

        /// <summary>
        ///     Consume the seat reservation
        /// </summary>
        /// <param name="response">The response from the matchmaking attempt</param>
        /// <param name="headers">Dictionary of headers to pass to the server</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we're consuming the seat from</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> in which we now have a seat via async task</returns>
        public async Task<ColyseusRoom<T>> ConsumeSeatReservation<T>(ColyseusMatchMakeResponse response,
            Dictionary<string, string> headers = null)
        {
            ColyseusRoom<T> room = new ColyseusRoom<T>(response.room.name)
            {
                Id = response.room.roomId,
                SessionId = response.sessionId
            };

            Dictionary<string, object> queryString = new Dictionary<string, object>();
            queryString.Add("sessionId", room.SessionId);
            Debug.Log("Setting Connection");
            room.SetConnection(CreateConnection(response.room.processId + "/" + room.Id, queryString, headers));
            Debug.Log("Connection Set?");
            TaskCompletionSource<ColyseusRoom<T>> tcs = new TaskCompletionSource<ColyseusRoom<T>>();

            void OnError(int code, string message)
            {
                Debug.Log("room error");
                room.OnError -= OnError;
                tcs.SetException(new CSAMatchMakeException(code, message));
            }

            void OnJoin()
            {
                Debug.Log("room joined");
                room.OnError -= OnError;
                tcs.TrySetResult(room);
            }

            room.OnError += OnError;
            room.OnJoin += OnJoin;
            Debug.Log("continue if any on add room");
            onAddRoom?.Invoke(room);
            Debug.Log("continue connect?");
#pragma warning disable 4014
            room.Connect(); //mandeng neng kene, so maybe the url is wrong??
#pragma warning restore 4014
            Debug.Log("connected??");
            return await tcs.Task;
        }

        /// <summary>
        ///     Create a match making request
        /// </summary>
        /// <param name="method">The type of request we're making (join, create, etc)</param>
        /// <param name="roomName">The name of the room we're trying to match</param>
        /// <param name="options">Dictionary of options to use in the match making process</param>
        /// <param name="headers">Dictionary of headers to pass to the server</param>
        /// <typeparam name="T">Type of <see cref="ColyseusRoom{T}" /> we want to match with</typeparam>
        /// <returns><see cref="ColyseusRoom{T}" /> we have matched with via async task</returns>
        /// <exception cref="Exception">Thrown if there is a network related error</exception>
        /// <exception cref="CSAMatchMakeException">Thrown if there is an error in the match making process on the server side</exception>
        protected async Task<ColyseusRoom<T>> CreateMatchMakeRequest<T>(string method, string roomName,
            Dictionary<string, object> options, Dictionary<string, string> headers)
        {
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }

            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            string json = await colyseusRequest.Request("POST", $"matchmake/{method}/{roomName}", options, headers);
            LSLog.Log($"Server Response: {json}");
            ColyseusMatchMakeResponse response =
                JsonUtility.FromJson<ColyseusMatchMakeResponse>(json);
            Debug.Log("Response made?");
            if (response == null)
            {
                Debug.Log("Response null");
                throw new Exception($"Error with request: {json}");
            }

            if (!string.IsNullOrEmpty(response.error))
            {
                Debug.Log("Is error response");
                throw new CSAMatchMakeException(response.code, response.error);
            }
            Debug.Log("Continue Consume Seat Reservation");
            return await ConsumeSeatReservation<T>(response, headers);
        }

        /// <summary>
        ///     Create a connection with a room
        /// </summary>
        /// <param name="path">Additional info used as the <see cref="UriBuilder.Path" /></param>
        /// <param name="options">Dictionary of options to use when connecting</param>
        /// <param name="headers">Dictionary of headers to pass when connecting</param>
        /// <returns></returns>
        protected ColyseusConnection CreateConnection(string path = "", Dictionary<string, object> options = null,
            Dictionary<string, string> headers = null)
        {
            if (options == null)
            {
                options = new Dictionary<string, object>();
            }

            List<string> list = new List<string>();
            foreach (KeyValuePair<string, object> item in options)
            {
                list.Add(item.Key + "=" + (item.Value != null ? Convert.ToString(item.Value) : "null"));
            }

            var uriBuilder = colyseusRequest.GetUriBuilder(path, string.Join("&", list.ToArray()));
            uriBuilder.Scheme = Endpoint.Scheme;

            return new ColyseusConnection(uriBuilder.ToString(), headers);
        }
    }
}