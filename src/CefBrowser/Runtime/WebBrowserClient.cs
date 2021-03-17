using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using ZeroMQ;
using Debug = UnityEngine.Debug;

namespace UnityWebBrowser
{
	/// <summary>
	///		Handles managing the process and worker thread
	/// </summary>
	[Serializable]
	public class WebBrowserClient : IDisposable
	{
		/// <summary>
		///		The initial URl the browser will start at
		/// </summary>
		[Header("Browser Settings")]
		[Tooltip("The initial URl the browser will start at")]
		public string initialUrl = "https://google.com";

		/// <summary>
		///		The width of the browser
		/// </summary>
		[Tooltip("The width of the browser")]
		public uint width = 1920;

		/// <summary>
		///		The height of the browser
		/// </summary>
		[Tooltip("The height of the browser")]
		public uint height = 1080;
		
		/// <summary>
		///		The background <see cref="Color"/> of the webpage
		/// </summary>
		public Color32 backgroundColor = new Color32(255, 255, 255, 255);

		/// <summary>
		///		The port to communicate with the browser process on
		/// </summary>
		[Header("IPC Settings")] 
		[Tooltip("The port to communicate with the browser process on")]
		public int port = 5555;

		/// <summary>
		///		The time between each frame sent the browser process
		/// </summary>
		[Tooltip("The time between each frame sent the browser process")]
		public float eventPollingTime = 0.04f;

		/// <summary>
		///		How many errors until we will just fail
		/// </summary>
		[Tooltip("How many errors until we will just fail")]
		public int errorsTillFail = 4;

		/// <summary>
		///		Show the CEF browser process console?
		/// </summary>
		[Tooltip("Show the CEF browser process console?")]
		public bool showProcessConsole;

		/// <summary>
		///		Texture that the browser will paint to
		/// </summary>
		public Texture2D BrowserTexture { get; private set; }

		/// <summary>
		///		<see cref="ILogger"/> that we log to
		/// </summary>
		public ILogger Logger { get; private set; } = Debug.unityLogger;
		private const string LoggingTag = "[Web Browser]";

		private Process serverProcess;
		private WebBrowserEventDispatcher eventDispatcher;
		
		private bool isRunning;

		private object pixelsLock = new object();
		private byte[] pixels;

		/// <summary>
		///		Raw pixel data of the browser.
		///		<para>Try to use <see cref="BrowserTexture"/> for displaying a texture instead of this!</para>
		/// </summary>
		public byte[] Pixels
		{
			get
			{
				lock (pixelsLock)
				{
					return pixels;
				}
			}
			private set
			{
				lock (pixelsLock)
				{
					pixels = value;
				}
			}
		}

		/// <summary>
		///		Inits the browser client
		/// </summary>
		/// <exception cref="FileNotFoundException"></exception>
		public void Init()
		{
			string cefProcessPath = WebBrowserUtils.GetCefProcessApplication();
			LogDebug($"Starting CEF browser process from {cefProcessPath}");

			if (!File.Exists(cefProcessPath))
			{
				LogError("The CEF browser process doesn't exist!");
				throw new FileNotFoundException("CEF browser process could not be found!");
			}

			//Start the server process
			serverProcess = new Process
			{
				StartInfo = new ProcessStartInfo(cefProcessPath, $"-width {width} -height {height} -url {initialUrl} -port {port} " +
				                                                 $"-bcr {backgroundColor.r} -bcg {backgroundColor.g} -bcb {backgroundColor.b} -bca {backgroundColor.a}")
				{
					CreateNoWindow = !showProcessConsole,
					UseShellExecute = showProcessConsole
				},
			};
			serverProcess.Start();

			BrowserTexture = new Texture2D((int)width, (int)height, TextureFormat.BGRA32, false, true);
			eventDispatcher = new WebBrowserEventDispatcher(new TimeSpan(0, 0, 4), port);
			eventDispatcher.StartDispatchingEvents();
		}

		/// <summary>
		///		Starts updating the <see cref="BrowserTexture"/> data
		/// </summary>
		/// <returns></returns>
		public IEnumerator Start()
		{
			LogDebug("Starting communications between CEF process and Unity...");
			isRunning = true;

			while (isRunning)
			{
				yield return new WaitForSecondsRealtime(eventPollingTime);

				eventDispatcher.QueueEvent(new PingEvent(), LoadPixels);

				byte[] pixelData = Pixels;

				if(pixelData == null || pixelData.Length == 0)
					continue;

				BrowserTexture.LoadRawTextureData(pixelData);
				BrowserTexture.Apply(false);
			}
		}

		private void LoadPixels(ZFrame frame)
		{
			Pixels = frame.Read();
			frame.Dispose();
		}

		#region Logging

		/// <summary>
		///		Logs a debug message
		/// </summary>
		/// <param name="message"></param>
		public void LogDebug(object message)
		{
			Logger.Log(LogType.Log, LoggingTag, message);
		}

		/// <summary>
		///		Logs a warning
		/// </summary>
		/// <param name="message"></param>
		public void LogWarning(object message)
		{
			Logger.LogWarning(LoggingTag, message);
		}

		/// <summary>
		///		Logs a error
		/// </summary>
		/// <param name="message"></param>
		public void LogError(object message)
		{
			Logger.LogError(LoggingTag, message);
		}

		/// <summary>
		///		Replaces the logger the web browser will use
		/// </summary>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void ReplaceLogger(ILogger logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		#endregion

		#region CEF process events

		/// <summary>
		///		Sends a keyboard event to the CEF process
		/// </summary>
		/// <param name="keysDown"></param>
		/// <param name="keysUp"></param>
		/// <param name="chars"></param>
		public void SendKeyboardEvent(int[] keysDown, int[] keysUp, string chars)
		{
			eventDispatcher.QueueEvent(new KeyboardEvent
			{
				KeysDown = keysDown,
				KeysUp = keysUp,
				Chars = chars
			}, HandelEvent);
		}

		///  <summary>
		/// 		Sends a mouse event to the CEF process
		///  </summary>
		///  <param name="mousePos"></param>
		public void SendMouseMoveEvent(Vector2 mousePos)
		{
			eventDispatcher.QueueEvent(new MouseMoveEvent
			{
				MouseX = (int)mousePos.x,
				MouseY = (int)mousePos.y
			}, HandelEvent);
		}

		///  <summary>
		/// 		Sends a mouse click event to the CEF process
		///  </summary>
		///  <param name="mousePos"></param>
		///  <param name="clickCount"></param>
		///  <param name="clickType"></param>
		///  <param name="eventType"></param>
		public void SendMouseClickEvent(Vector2 mousePos, int clickCount, MouseClickType clickType, MouseEventType eventType)
		{
			eventDispatcher.QueueEvent(new MouseClickEvent
			{
				MouseX = (int)mousePos.x,
				MouseY = (int)mousePos.y,
				MouseClickCount = clickCount,
				MouseClickType = clickType,
				MouseEventType = eventType
			}, HandelEvent);
		}

		/// <summary>
		///		Sends a mouse scroll event
		/// </summary>
		/// <param name="mouseX"></param>
		/// <param name="mouseY"></param>
		/// <param name="mouseScroll"></param>
		public void SendMouseScrollEvent(int mouseX, int mouseY, int mouseScroll)
		{
			eventDispatcher.QueueEvent(new MouseScrollEvent
			{
				MouseScroll = mouseScroll,
				MouseX = mouseX,
				MouseY = mouseY
			}, HandelEvent);
		}

		/// <summary>
		///		Sends a button event
		/// </summary>
		/// <param name="buttonType"></param>
		/// <param name="url"></param>
		public void SendButtonEvent(ButtonType buttonType, string url = null)
		{
			eventDispatcher.QueueEvent(new ButtonEvent
			{
				ButtonType = buttonType,
				UrlToNavigate = url
			}, HandelEvent);
		}

		private void HandelEvent(ZFrame frame)
		{
			frame.Dispose();
		}

		#endregion

		#region Destroying

		~WebBrowserClient()
		{
			ReleaseResources();
		}
		
		/// <summary>
		///		Destroys this <see cref="WebBrowserClient"/> instance
		/// </summary>
		public void Dispose()
		{
			ReleaseResources();
			GC.SuppressFinalize(this);
		}

		private void ReleaseResources()
		{
			if(!isRunning)
				return;

			eventDispatcher.Dispose();

			if(!serverProcess.HasExited)
				serverProcess.Kill();
			serverProcess.Dispose();

			LogDebug("Web browser shutdown.");
		}

		#endregion
	}
}