namespace QBX.DevelopmentEnvironment.Dialogs;

public interface IFocusContext
{
	void SetFocus(Widget widget);
	bool TrySetFocus(byte accessKey);
}
