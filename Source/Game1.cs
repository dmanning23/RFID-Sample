using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using ToastBuddyLib;
using ResolutionBuddy;
using FontBuddyLib;
using Phidgets; //Needed for the RFID class and the PhidgetException class
using Phidgets.Events; //Needed for the phidget event handling classes

namespace RFIDSample
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
	
		ToastBuddy m_Messages;

		FontBuddy InstructionFont;

		RFID rfid; //Declare an RFID object

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
			Resolution.Init(ref graphics);
			Content.RootDirectory = "Content";

			Resolution.SetDesiredResolution(1280, 720);
			Resolution.SetScreenResolution(1280, 720, true);

			m_Messages = new ToastBuddy(this, "ArialBlack24", UpperRight, Resolution.TransformationMatrix);
			Components.Add(m_Messages);

			//initialize our Phidgets RFID reader and hook the event handlers
			rfid = new RFID();
			rfid.Attach += new AttachEventHandler(rfid_Attach);
			rfid.Detach += new DetachEventHandler(rfid_Detach);
			rfid.Error += new ErrorEventHandler(rfid_Error);

			rfid.Tag += new TagEventHandler(rfid_Tag);
			rfid.TagLost += new TagEventHandler(rfid_TagLost);
			rfid.open();
		}

		public Vector2 UpperRight()
		{
			return new Vector2(Resolution.TitleSafeArea.Right, Resolution.TitleSafeArea.Top);
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			InstructionFont = new FontBuddy();
			InstructionFont.Font = Content.Load<SpriteFont>("ArialBlack24");
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			//close the phidget and dispose of the object
			rfid.close();
			rfid = null;

			base.OnExiting(sender, args);
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// For Mobile devices, this logic will close the Game when the Back button is pressed
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
			    (Keyboard.GetState().IsKeyDown(Keys.Escape)))
			{
				Exit();
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			#if WINDOWS
			Resolution.ResetViewport();
			#endif

			spriteBatch.Begin();

			//TODO: Add your drawing code here
			InstructionFont.Write("Attach an RFID reader and scan tags to pop up messages",
			                      new Vector2(Resolution.TitleSafeArea.Left, Resolution.TitleSafeArea.Top),
			                      Justify.Left,
			                      0.75f,
			                      Color.White,
			                      spriteBatch,
			                      0.0);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		//attach event handler...display the serial number of the attached RFID phidget
		void rfid_Attach(object sender, AttachEventArgs e)
		{
			//Get the toast message component
			IServiceProvider services = Services;
			IMessageDisplay messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
			rfid.Antenna = true;
			rfid.LED = true;

			//pop up a message
			messageDisplay.ShowFormattedMessage("RFIDReader {0} attached!", e.Device.SerialNumber.ToString());
		}

		//detach event handler...display the serial number of the detached RFID phidget
		void rfid_Detach(object sender, DetachEventArgs e)
		{
			IServiceProvider services = Services;
			IMessageDisplay messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
			messageDisplay.ShowFormattedMessage("RFID reader {0} detached!", e.Device.SerialNumber.ToString());
		}

		//Error event handler...display the error description string
		void rfid_Error(object sender, ErrorEventArgs e)
		{
			IServiceProvider services = Services;
			IMessageDisplay messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
			messageDisplay.ShowFormattedMessage(e.Description);
		}

		//Print the tag code of the scanned tag
		void rfid_Tag(object sender, TagEventArgs e)
		{
			IServiceProvider services = Services;
			IMessageDisplay messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
			messageDisplay.ShowFormattedMessage("Tag {0} scanned", e.Tag);
		}

		//print the tag code for the tag that was just lost
		void rfid_TagLost(object sender, TagEventArgs e)
		{
			IServiceProvider services = Services;
			IMessageDisplay messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
			messageDisplay.ShowFormattedMessage("Tag {0} lost", e.Tag);
		}
	}
}
