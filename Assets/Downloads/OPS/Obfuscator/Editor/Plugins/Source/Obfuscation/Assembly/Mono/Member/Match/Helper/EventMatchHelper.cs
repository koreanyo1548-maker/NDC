using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//OPS
using OPS.Mono.Cecil;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Match
{
    internal class EventMatchHelper
    {
        // Nice to know: https://github.com/mono/mono/blob/master/mcs/tools/linker/Mono.Linker.Steps/TypeMapStep.cs

        public static bool EventMatch(EventDefinition candidate, EventDefinition @event)
        {
            if (candidate == null && @event == null)
            {
                return true;
            }

            if (candidate == null || @event == null)
            {
                return false;
            }

            if (candidate.Name != @event.Name)
                return false;

            if (!TypeMatchHelper.TypeMatch(candidate.EventType, @event.EventType))
                return false;

            // Note: Who cares about the methods? They might be different!
            /*if (!(Match.MethodMatchHelper.MethodMatch(candidate.AddMethod, @event.AddMethod)
                || Match.MethodMatchHelper.MethodMatch(@event.AddMethod, candidate.AddMethod)))
            {
                return false;
            }

            if (!(Match.MethodMatchHelper.MethodMatch(candidate.RemoveMethod, @event.RemoveMethod)
                || Match.MethodMatchHelper.MethodMatch(@event.RemoveMethod, candidate.RemoveMethod)))
            {
                return false;
            }*/

            return true;            
        }
    }
}
