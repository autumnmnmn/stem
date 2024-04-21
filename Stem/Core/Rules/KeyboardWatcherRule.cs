using Utilities.Extensions;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Stem.Rules;

public struct KeyboardWatcher {
    // Null char is backspace.
    public IKeyboardWatcher Implementation;

    public bool isCapturing = false;

    public KeyboardWatcher(IKeyboardWatcher implementation) {
        Implementation = implementation;
    }
}

public class StandardKeyboardWatcher : IKeyboardWatcher {
    private string __string;

    private readonly Action<string> __sendString;

    private readonly Func<string> __fetchString;

    public StandardKeyboardWatcher(Func<string> source, Action<string> destination) {
        __fetchString = source;
        __sendString = destination;
        __string = "";
    }

    public void OnTickStart() => __string = __fetchString();

    public void OnTickEnd() => __sendString(__string);

    public void OnCharacter(char @char, IKeyboardWatcher.ContextKeys context) {
        __string = $"{__string}{@char}";
    }

    public void OnBackspace(IKeyboardWatcher.ContextKeys context) {
        __string = string.Concat(__string.SkipLast(1));
    }
}

public interface IKeyboardWatcher {
    public record struct ContextKeys(bool ShiftIsHeld, bool ControlIsHeld, bool AltIsHeld);

    public void OnTickStart() {}

    public void OnCharacter(char @char, ContextKeys context) {}

    public void OnBackspace(ContextKeys context) {}

    public bool InterceptEnter(ContextKeys context) => false;

    public bool InterceptSpace(ContextKeys context) => false;

    public void OnArrows(bool left, bool right, bool up, bool down, ContextKeys context) {}

    public void OnTickEnd() {}

}

public class KeyboardWatcherRule : Rule {
    protected override Archetype Archetype => Archetype.Create<KeyboardWatcher>();

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        var keyboard = Global.Get<KeyboardState>();
        if (!keyboard.IsAnyKeyDown) return;

        var input_aspects = entities.Select(store.GetAspect<KeyboardWatcher>).Where(input => input.isCapturing).ToList();
        if (input_aspects.Count == 0) return;

        var inputs = input_aspects.Select(aspect => aspect.Implementation);

        inputs.ForEach(input => input.OnTickStart());

        var caps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Where(character => keyboard.IsKeyPressed((Keys)character)).ToList();
        var others = "0123456789`-=[]\\;',./".Indexed().Where(itemIndex => keyboard.IsKeyPressed((Keys)itemIndex.Item)).ToList();
        
        var other_others = ")!@#$%^&*(~_+{}|:\"<>?".ToList();

        var presses = new List<char>();

        var context = new IKeyboardWatcher.ContextKeys(
            keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift),
            keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl),
            keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)
        );

        if (keyboard.IsKeyPressed(Keys.Backspace)) {
            inputs.ForEach(input => input.OnBackspace(context));
        }
        if (keyboard.IsKeyPressed(Keys.Space)) {
            inputs.ForEach(input => {
                if (input.InterceptSpace(context)) return;
                input.OnCharacter(' ', context);
            });
        }
        if (keyboard.IsKeyPressed(Keys.Enter)) {
            inputs.ForEach(input => {
                if (input.InterceptEnter(context)) return;
                input.OnCharacter('\n', context);
            });
        }

        if (!context.ShiftIsHeld) {
            presses.AddRange(caps.Select(cap => (char)(cap + 32)));
            presses.AddRange(others.Select(itemIndex => itemIndex.Item));
        } else {
            presses.AddRange(caps);
            presses.AddRange(others.Select(itemIndex => other_others[itemIndex.Index]));
        }

        if (presses.Count != 0) { 
            inputs.ForEach(input => presses.ForEach(press => input.OnCharacter(press, context)));
        }

        inputs.ForEach(input => input.OnTickEnd());
    }
}
