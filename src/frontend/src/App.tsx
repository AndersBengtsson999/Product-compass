import {
  useState,
  useEffect,
  useRef,
  type FormEvent,
  type ComponentType,
} from "react";
import { Link, NavLink, Route, Routes } from "react-router-dom";
import {
  ArrowRight,
  ArrowUpRight,
  Check,
  Compass,
  Flag,
  Layers3,
  LayoutDashboard,
  Lightbulb,
  LockKeyhole,
  MessageSquare,
  Plus,
  Sparkles,
  X,
} from "lucide-react";
import { loadWorkspace, type WorkspaceOverview } from "./api";

type Icon = ComponentType<{
  size?: number;
  strokeWidth?: number;
  className?: string;
}>;
const sections: {
  path: string;
  label: string;
  icon: Icon;
  description: string;
}[] = [
  {
    path: "/needs",
    label: "Customer needs",
    icon: Lightbulb,
    description: "Understand the problems worth solving.",
  },
  {
    path: "/signals",
    label: "Customer signals",
    icon: MessageSquare,
    description: "Bring the voice of your customers together.",
  },
  {
    path: "/initiatives",
    label: "Initiatives",
    icon: Flag,
    description: "Turn customer understanding into a direction.",
  },
];

export default function App() {
  const [workspace, setWorkspace] = useState<WorkspaceOverview | null>(null);
  const [showConnection, setShowConnection] = useState(false);
  return (
    <div className="app-shell">
      <a className="skip-link" href="#main">
        Skip to content
      </a>
      <aside className="sidebar">
        <Link className="brand" to="/">
          <span className="brand-mark">
            <Compass size={25} />
          </span>
          <span>
            product compass<span className="brand-dot">.</span>
          </span>
        </Link>
        <div className="workspace-card">
          <span className="workspace-avatar">
            {workspace?.organizationName.slice(0, 1) ?? "P"}
          </span>
          <div>
            <strong>{workspace?.organizationName ?? "Your workspace"}</strong>
            <span>
              {workspace ? "Connected organization" : "Foundation preview"}
            </span>
          </div>
          <Layers3 size={16} />
        </div>
        <div className="nav-heading">WORKSPACE</div>
        <nav aria-label="Workspace navigation">
          <NavLink end to="/" className="nav-item">
            <LayoutDashboard size={19} />
            Overview
          </NavLink>
          {sections.map((s) => (
            <NavLink key={s.path} to={s.path} className="nav-item">
              <s.icon size={19} />
              {s.label}
            </NavLink>
          ))}
          <div className="nav-divider" />
          <NavLink to="/analysis" className="nav-item">
            <Sparkles size={19} />
            AI discovery<span className="tiny-label">AI</span>
          </NavLink>
        </nav>
        <div className="sidebar-bottom">
          <div className="quiet-note">
            <span className="small-compass">
              <Compass size={20} />
            </span>
            <strong>
              Good products start
              <br />
              with understanding.
            </strong>
            <p>Keep customer needs at the heart of every decision.</p>
          </div>
          <div className="profile">
            <span className="profile-avatar">PC</span>
            <div>
              <strong>Product Compass</strong>
              <span>Local development · v0.1</span>
            </div>
          </div>
        </div>
      </aside>
      <div className="main-column">
        <header className="topbar">
          <span className="breadcrumb">
            Workspace <span>/</span>{" "}
            <strong>{workspace?.workspaceName ?? "Product workspace"}</strong>
          </span>
          <span className="build-badge">
            <span />
            Foundation
          </span>
        </header>
        <main id="main" tabIndex={-1}>
          <Routes>
            <Route
              path="/"
              element={
                <Overview
                  connected={!!workspace}
                  onConnect={() => setShowConnection(true)}
                />
              }
            />
            {sections.map((s) => (
              <Route
                key={s.path}
                path={s.path}
                element={<FeaturePage section={s} />}
              />
            ))}
            <Route path="/analysis" element={<AnalysisPage />} />
            <Route
              path="*"
              element={
                <div className="empty-panel">
                  <Compass size={36} />
                  <h1>Page not found</h1>
                  <p>This page isn’t part of your workspace.</p>
                  <Link className="primary-button" to="/">
                    Back to overview <ArrowRight size={16} />
                  </Link>
                </div>
              }
            />
          </Routes>
        </main>
        <footer>
          <span>
            <Compass size={14} /> A clearer direction, together.
          </span>
          <span>Product Compass · Foundation release</span>
        </footer>
      </div>
      {showConnection && (
        <ConnectionDialog
          onClose={() => setShowConnection(false)}
          onConnected={(value) => {
            setWorkspace(value);
            setShowConnection(false);
          }}
        />
      )}
    </div>
  );
}

function Overview({
  connected,
  onConnect,
}: {
  connected: boolean;
  onConnect: () => void;
}) {
  return (
    <>
      <div className="page-heading">
        <div>
          <div className="eyebrow">YOUR PRODUCT, IN PERSPECTIVE</div>
          <h1>A clearer path forward.</h1>
          <p>Connect what customers tell you to what your team builds next.</p>
        </div>
        <span className="outline-badge">
          <span className="status-dot" />
          {connected ? "Workspace connected" : "Workspace preview"}
        </span>
      </div>
      <section className="welcome-panel">
        <div className="welcome-copy">
          <span className="welcome-label">
            <Compass size={16} /> BUILT AROUND CUSTOMER NEEDS
          </span>
          <h2>
            Start with understanding.
            <br />
            Build with confidence.
          </h2>
          <p>
            One shared space for customer feedback, the needs behind it, and the
            initiatives that move your product forward.
          </p>
          <Link to="/needs" className="light-button">
            Explore customer needs <ArrowRight size={17} />
          </Link>
        </div>
        <div className="compass-art" aria-hidden="true">
          <div className="orbit orbit-one" />
          <div className="orbit orbit-two" />
          <div className="orbit orbit-three" />
          <div className="orbit-line horizontal" />
          <div className="orbit-line vertical" />
          <span className="north">N</span>
          <div className="needle" />
          <span className="art-tag tag-signals">
            <MessageSquare size={14} /> Signals
          </span>
          <span className="art-tag tag-needs">
            <Lightbulb size={15} /> Customer needs
          </span>
          <span className="art-tag tag-initiatives">
            <Flag size={14} /> Initiatives
          </span>
          <span className="orbit-point point-one" />
          <span className="orbit-point point-two" />
        </div>
      </section>
      <div className="section-heading">
        <div>
          <h2>Your discovery workspace</h2>
          <p>A connected journey from evidence to action.</p>
        </div>
        <span className="muted-small">THREE PARTS. ONE SHARED DIRECTION.</span>
      </div>
      <section className="feature-grid" aria-label="Discovery workflow">
        {sections.map((s, i) => (
          <Link
            to={s.path}
            className={"feature-card " + (i === 0 ? "featured" : "")}
            key={s.path}
          >
            <div className="card-top">
              <span className={"feature-icon icon-" + i}>
                <s.icon size={22} />
              </span>
              <ArrowUpRight size={18} />
            </div>
            <span className="card-label">
              {i === 0
                ? "THE HEART OF YOUR PRODUCT"
                : i === 1
                  ? "YOUR CUSTOMER EVIDENCE"
                  : "WHERE INSIGHT BECOMES ACTION"}
            </span>
            <h3>{s.label}</h3>
            <p>{s.description}</p>
            <div className="card-bottom">
              <span className="planned-dot" />
              Coming next
              <span>
                Explore <ArrowRight size={14} />
              </span>
            </div>
          </Link>
        ))}
      </section>
      <section className="lower-grid">
        <div className="getting-started">
          <div className="section-heading">
            <h2>A foundation for your team</h2>
            <span className="step-badge">
              {connected ? "02 / 03" : "01 / 03"}
            </span>
          </div>
          <div className="setup-row">
            <span className="step-circle complete">
              <Check size={16} />
            </span>
            <div>
              <strong>Make space for discovery</strong>
              <p>Your application shell and navigation are ready.</p>
            </div>
            <span className="done-label">Ready</span>
          </div>
          <div className="setup-row">
            <span className="step-circle">2</span>
            <div>
              <strong>Connect your workspace</strong>
              <p>
                {connected
                  ? "Organization access verified through the API."
                  : "Verify access to your PostgreSQL-backed organization."}
              </p>
            </div>
            {connected ? (
              <Check size={18} className="green" />
            ) : (
              <button className="text-button" onClick={onConnect}>
                Connect <ArrowRight size={15} />
              </button>
            )}
          </div>
          <div className="setup-row">
            <span className="step-circle">3</span>
            <div>
              <strong>Bring in your first customer signals</strong>
              <p>Signal capture and linking arrive in the next milestone.</p>
            </div>
            <span className="upcoming-label">Planned</span>
          </div>
        </div>
        <div className="ai-note">
          <span className="ai-icon">
            <Sparkles size={22} />
          </span>
          <span className="outline-badge">ON THE HORIZON</span>
          <h2>
            A thought partner.
            <br />A human decision.
          </h2>
          <p>
            AI will help uncover possible needs in your signals. Every
            suggestion will stay clearly labeled until your team reviews it.
          </p>
          <Link to="/analysis">
            About AI discovery <ArrowRight size={16} />
          </Link>
        </div>
      </section>
    </>
  );
}

function FeaturePage({ section }: { section: (typeof sections)[number] }) {
  const details: Record<string, string> = {
    "/needs":
      "Create customer needs, link supporting signals, and build a shared understanding of the problems that matter.",
    "/signals":
      "Capture and edit customer feedback, then connect multiple signals to the needs they reveal.",
    "/initiatives":
      "Create initiatives linked to customer needs so the reason behind your work stays visible.",
  };
  return (
    <>
      <div className="page-heading">
        <div>
          <div className="eyebrow">DISCOVERY WORKSPACE</div>
          <h1>{section.label}</h1>
          <p>{section.description}</p>
        </div>
        <span className="outline-badge">Next milestone</span>
      </div>
      <section className="empty-panel">
        <span className="empty-icon">
          <section.icon size={32} />
        </span>
        <span className="card-label">A PLACE FOR WHAT MATTERS</span>
        <h2>
          {section.label === "Customer needs"
            ? "Understanding starts here."
            : section.label === "Customer signals"
              ? "Every customer voice is a signal."
              : "Give your next move a purpose."}
        </h2>
        <p>{details[section.path]}</p>
        <div className="implementation-note">
          <Plus size={16} /> This workflow is planned. Creation and editing are
          not available yet.
        </div>
        <Link to="/" className="secondary-button">
          Back to overview <ArrowRight size={16} />
        </Link>
      </section>
    </>
  );
}
function AnalysisPage() {
  return (
    <>
      <div className="page-heading">
        <div>
          <div className="eyebrow">ASSISTED DISCOVERY</div>
          <h1>AI discovery</h1>
          <p>Find patterns. Ask better questions. Keep your team in control.</p>
        </div>
        <span className="outline-badge">
          <Sparkles size={14} /> Planned
        </span>
      </div>
      <section className="empty-panel">
        <span className="empty-icon purple">
          <Sparkles size={32} />
        </span>
        <span className="suggestion-badge">
          AI suggestion ≠ verified customer need
        </span>
        <h2>Explore the needs behind the words.</h2>
        <p>
          AI analysis will propose underlying needs from customer signals, with
          supporting evidence for your team to review. Suggestions will never
          become verified facts automatically.
        </p>
        <div className="implementation-note">
          <LockKeyhole size={16} /> No AI provider is connected. No data is sent
          for analysis.
        </div>
        <Link to="/" className="secondary-button">
          Back to overview <ArrowRight size={16} />
        </Link>
      </section>
    </>
  );
}
function ConnectionDialog({
  onClose,
  onConnected,
}: {
  onClose: () => void;
  onConnected: (workspace: WorkspaceOverview) => void;
}) {
  const dialog = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    dialog.current?.showModal();
  }, []);
  const [organizationId, setOrganizationId] = useState(
    "11111111-1111-1111-1111-111111111111",
  );
  const [token, setToken] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  async function submit(event: FormEvent) {
    event.preventDefault();
    setError("");
    setBusy(true);
    try {
      onConnected(await loadWorkspace(organizationId.trim(), token.trim()));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Unable to connect. Please try again.",
      );
    } finally {
      setBusy(false);
    }
  }
  return (
    <dialog
      ref={dialog}
      onCancel={(e) => {
        e.preventDefault();
        if (!busy) onClose();
      }}
      aria-labelledby="connection-title"
      className="connection-dialog"
      onKeyDown={(e) => {
        if (e.key === "Escape" && !busy) onClose();
      }}
    >
      <button
        aria-label="Close connection dialog"
        className="close-button"
        onClick={onClose}
        disabled={busy}
      >
        <X size={20} />
      </button>
      <span className="empty-icon">
        <LockKeyhole size={24} />
      </span>
      <h2 id="connection-title">Connect your workspace</h2>
      {import.meta.env.DEV ? (
        <>
          <p>
            Use a local development token to verify your organization
            membership. See the README for setup.
          </p>
          <form onSubmit={submit}>
            <label htmlFor="org">Organization ID</label>
            <input
              id="org"
              autoFocus
              required
              value={organizationId}
              onChange={(e) => setOrganizationId(e.target.value)}
            />
            <label htmlFor="token">Development access token</label>
            <input
              id="token"
              type="password"
              autoComplete="off"
              required
              value={token}
              onChange={(e) => setToken(e.target.value)}
            />
            <p className="form-help">
              Used for this request only. Never stored in your browser.
            </p>
            {error && (
              <p role="alert" className="error-message">
                {error}
              </p>
            )}
            <button className="primary-button" disabled={busy}>
              {busy ? "Verifying access…" : "Connect workspace"}
              <ArrowRight size={16} />
            </button>
          </form>
        </>
      ) : (
        <p>
          Organization sign-in will be added in a future milestone. This build
          is a foundation preview.
        </p>
      )}
    </dialog>
  );
}
