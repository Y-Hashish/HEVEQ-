using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Models
{
    /// <summary>
    /// A single prior turn in the Sticky Chat thread.
    /// The Angular client resends the full thread on every follow-up because the API is
    /// stateless — this lets a one-word reply like "Giza" be resolved against whatever
    /// clarifying question the assistant last asked, with no server-side session storage.
    /// </summary>
    /// <param name="Role">"user" | "assistant"</param>
    public sealed record ConversationTurn(string Role, string Content);
}
