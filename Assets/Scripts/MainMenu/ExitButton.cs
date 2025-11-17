using Godot;
using System;
using System.Diagnostics;

public partial class ExitButton : Button
{

	private GlobalSignals globalSignals;
	public override void _Ready()
	{
		globalSignals = GetNode("/root/GlobalSignals") as GlobalSignals;
		this.ButtonDown += ExitButtonOnClick;
	}
	private void ExitButtonOnClick()
	{
		// Получаем путь к текущему исполняемому файлу
		string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

		// Запускаем новый процесс игры
		Process.Start(exePath);

		// Завершаем текущий процесс
		GetTree().Quit();
	}
}
