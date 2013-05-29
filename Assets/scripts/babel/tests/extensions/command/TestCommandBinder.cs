using System;
using System.Timers;
using NUnit.Framework;
using babel.extensions.command.api;
using babel.extensions.command.impl;
using babel.extensions.injector.api;
using babel.extensions.injector.impl;

using babel.framework.impl;


namespace babel.unittests
{
	[TestFixture()]
	public class TestCommandBinder
	{
		IInjectionBinder injectionBinder;
		ICommandBinder commandBinder;

		[SetUp]
		public void SetUp()
		{
			injectionBinder = new InjectionBinder();
			injectionBinder.Bind<IInjectionBinder> ().AsValue (injectionBinder);
			injectionBinder.Bind<ICommandBinder> ().To<CommandBinder> ().AsSingleton ();
			commandBinder = injectionBinder.GetInstance<ICommandBinder> () as ICommandBinder;
		}

		[Test]
		public void TestExecuteWithInjection ()
		{
			//CommandWithInjection requires an ISimpleInterface
			injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer> ().AsSingleton();

			//Bind the trigger to the command
			commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>();
			commandBinder.ReactTo (SomeEnum.ONE);

			//The command should set the value to 100
			ISimpleInterface instance = injectionBinder.GetInstance<ISimpleInterface>() as ISimpleInterface;
			Assert.AreEqual (100, instance.intValue);
		}

		[Test]
		public void TestMultipleCommands ()
		{
			//CommandWithInjection requires an ISimpleInterface
			injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer> ().AsSingleton();

			//Bind the trigger to the command
			commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>().To<CommandWithExecute>().To<CommandWithoutExecute>();

			TestDelegate testDelegate = delegate 
			{
				commandBinder.ReactTo (SomeEnum.ONE);
			};

			//That the exception is thrown demonstrates that the last command ran
			CommandException ex = Assert.Throws<CommandException> (testDelegate);
			Assert.AreEqual (ex.type, CommandExceptionType.EXECUTE_OVERRIDE);

			//That the value is 100 demonstrates that the first command ran
			ISimpleInterface instance = injectionBinder.GetInstance<ISimpleInterface>() as ISimpleInterface;
			Assert.AreEqual (100, instance.intValue);
		}

		[Test]
		public void TestNotOnce()
		{
			//CommandWithInjection requires an ISimpleInterface
			injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer> ().AsSingleton();

			//Bind the trigger to the command
			commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>();

			ICommandBinding binding = commandBinder.GetBinding (SomeEnum.ONE) as ICommandBinding;
			Assert.IsNotNull (binding);

			commandBinder.ReactTo (SomeEnum.ONE);

			ICommandBinding binding2 = commandBinder.GetBinding (SomeEnum.ONE) as ICommandBinding;
			Assert.IsNotNull (binding2);
		}

		[Test]
		public void TestOnce ()
		{
			//CommandWithInjection requires an ISimpleInterface
			injectionBinder.Bind<ISimpleInterface>().To<SimpleInterfaceImplementer> ().AsSingleton();

			//Bind the trigger to the command
			commandBinder.Bind(SomeEnum.ONE).To<CommandWithInjection>().Once();

			ICommandBinding binding = commandBinder.GetBinding (SomeEnum.ONE) as ICommandBinding;
			Assert.IsNotNull (binding);

			commandBinder.ReactTo (SomeEnum.ONE);

			ICommandBinding binding2 = commandBinder.GetBinding (SomeEnum.ONE) as ICommandBinding;
			Assert.IsNull (binding2);
		}

		//TODO: figure out how to do async tests
		/*
		[Test]
		public async void TestAsyncCommand()
		{
			injectionBinder.Bind<Timer>().To<Timer> ();
			commandBinder.Bind (SomeEnum.ONE).To<AsynchCommand> ();
			Task<bool> answer = commandBinder.ReactTo (SomeEnum.ONE);

			//Assert.Throws<Exception> ( await );
		}
		*/
	}
}

