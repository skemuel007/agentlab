
import { useState } from "react";
import "./App.css";

type Message = {
  role: "user" | "assistant";
  content: string;
};

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);

  async function sendMessage() {
    const message = input.trim();
    if (!message || loading) return;

    setMessages(previous => [
      ...previous,
      { role: "user", content: message }
    ]);

    setInput("");
    setLoading(true);

    try {
      const response = await fetch(
        "http://localhost:5080/api/chat",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json"
          },
          body: JSON.stringify({ message })
        }
      );

      if (!response.ok) {
        throw new Error("The chat request failed.");
      }

      const data: { reply: string } = await response.json();

      setMessages(previous => [
        ...previous,
        { role: "assistant", content: data.reply }
      ]);
    } catch {
      setMessages(previous => [
        ...previous,
        {
          role: "assistant",
          content: "Unable to connect to the API."
        }
      ]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main style={{ maxWidth: 720, margin: "2rem auto" }}>
      <h1>AgentLab</h1>
      <p>Our first full-stack AI application</p>

      <section aria-label="Conversation">
        {messages.map((message, index) => (
          <div key={index}>
            <strong>
              {message.role === "user" ? "You" : "Agent"}
            </strong>
            <p>{message.content}</p>
          </div>
        ))}
        {loading && <p>Agent is responding...</p>}
      </section>

      <form onSubmit={event => {
        event.preventDefault();
        void sendMessage();
      }}>
        <input
          aria-label="Your message"
          value={input}
          onChange={event => setInput(event.target.value)}
          placeholder="Ask AgentLab something..."
        />
        <button disabled={loading || !input.trim()}>
          Send
        </button>
      </form>
    </main>
  );
}
