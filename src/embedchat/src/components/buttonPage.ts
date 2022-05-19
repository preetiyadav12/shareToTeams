const template = document.createElement("template")!;
template.innerHTML = `
    <div class="teams-embed-button-page">
        <button class="teams-embed-button-page-btn">
            <img class="teams-embed-button-page-btn-img" alt="Microsoft Teams" src="" />
            <span class="teams-embed-button-page-btn-text"></span>
        </button>
    </div>`;

export class ButtonPage extends HTMLElement {
  private text: string;
  private callback: any;
  constructor(text: string, callback: any) {
    super();
    this.text = text;
    this.callback = callback;
    this.render();
  }

  render() {
    const teamsIcon =
      "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hsaW5rIiB2aWV3Qm94PSIwIDAgMTAyNCAxMDI0Ij4KICAgICAgPGRlZnM+CiAgICAgICAgPGxpbmVhckdyYWRpZW50IGlkPSJwbGF0ZS1maWxsIiB4MT0iLS4yIiB5MT0iLS4yIiB4Mj0iLjgiIHkyPSIuOCI+CiAgICAgICAgICA8c3RvcCBvZmZzZXQ9IjAiIHN0b3AtY29sb3I9IiM1YTYyYzQiPjwvc3RvcD4KICAgICAgICAgIDxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzM5NDBhYiI+PC9zdG9wPgogICAgICAgIDwvbGluZWFyR3JhZGllbnQ+CiAgICAgICAgPHN0eWxlPgogICAgICAgICAgLmNscy0xe2ZpbGw6IzUwNTljOX0uY2xzLTJ7ZmlsbDojN2I4M2VifQogICAgICAgIDwvc3R5bGU+CiAgICAgICAgPGZpbHRlciBpZD0icGVyc29uLXNoYWRvdyIgeD0iLTUwJSIgeT0iLTUwJSIgd2lkdGg9IjMwMCUiIGhlaWdodD0iMzAwJSI+CiAgICAgICAgICA8ZmVHYXVzc2lhbkJsdXIgaW49IlNvdXJjZUFscGhhIiBzdGREZXZpYXRpb249IjI1Ij48L2ZlR2F1c3NpYW5CbHVyPgogICAgICAgICAgPGZlT2Zmc2V0IGR5PSIyNSI+PC9mZU9mZnNldD4KICAgICAgICAgIDxmZUNvbXBvbmVudFRyYW5zZmVyPgogICAgICAgICAgICA8ZmVGdW5jQSB0eXBlPSJsaW5lYXIiIHNsb3BlPSIuMjUiPjwvZmVGdW5jQT4KICAgICAgICAgIDwvZmVDb21wb25lbnRUcmFuc2Zlcj4KICAgICAgICAgIDxmZU1lcmdlPgogICAgICAgICAgICA8ZmVNZXJnZU5vZGU+PC9mZU1lcmdlTm9kZT4KICAgICAgICAgICAgPGZlTWVyZ2VOb2RlIGluPSJTb3VyY2VHcmFwaGljIj48L2ZlTWVyZ2VOb2RlPgogICAgICAgICAgPC9mZU1lcmdlPgogICAgICAgIDwvZmlsdGVyPgoKCiAgICAgICAgPGZpbHRlciBpZD0iYmFjay1wbGF0ZS1zaGFkb3ciIHg9Ii01MCUiIHk9Ii01MCUiIHdpZHRoPSIzMDAlIiBoZWlnaHQ9IjMwMCUiPgogICAgICAgICAgCgk8ZmVHYXVzc2lhbkJsdXIgaW49IlNvdXJjZUFscGhhIiBzdGREZXZpYXRpb249IjI0Ij48L2ZlR2F1c3NpYW5CbHVyPgoJICA8ZmVPZmZzZXQgZHg9IjIiIGR5PSIyNCI+PC9mZU9mZnNldD4KICAgICAgICAgIDxmZUNvbXBvbmVudFRyYW5zZmVyPgogICAgICAgICAgPGZlRnVuY0EgdHlwZT0ibGluZWFyIiBzbG9wZT0iLjYiPjwvZmVGdW5jQT4KCiAgICAgICAgICA8L2ZlQ29tcG9uZW50VHJhbnNmZXI+CiAgICAgICAgICA8ZmVNZXJnZT4KICAgICAgICAgICAgPGZlTWVyZ2VOb2RlPjwvZmVNZXJnZU5vZGU+CiAgICAgICAgICAgIDxmZU1lcmdlTm9kZSBpbj0iU291cmNlR3JhcGhpYyI+PC9mZU1lcmdlTm9kZT4KICAgICAgICAgIDwvZmVNZXJnZT4KICAgICAgICA8L2ZpbHRlcj4KICAgICAgICA8ZmlsdGVyIGlkPSJ0ZWUtc2hhZG93IiB4PSItNTAlIiB5PSItNTAlIiB3aWR0aD0iMjUwJSIgaGVpZ2h0PSIyNTAlIj4KICAgICAgICAgIDxmZUdhdXNzaWFuQmx1ciBpbj0iU291cmNlQWxwaGEiIHN0ZERldmlhdGlvbj0iMTIiPjwvZmVHYXVzc2lhbkJsdXI+CiAgICAgICAgICA8ZmVPZmZzZXQgZHg9IjEwIiBkeT0iMjAiPjwvZmVPZmZzZXQ+CiAgICAgICAgICA8ZmVDb21wb25lbnRUcmFuc2Zlcj4KICAgICAgICAgICAgPGZlRnVuY0EgdHlwZT0ibGluZWFyIiBzbG9wZT0iLjIiPjwvZmVGdW5jQT4KICAgICAgICAgIDwvZmVDb21wb25lbnRUcmFuc2Zlcj4KICAgICAgICAgIDxmZU1lcmdlPgogICAgICAgICAgICA8ZmVNZXJnZU5vZGU+PC9mZU1lcmdlTm9kZT4KICAgICAgICAgICAgPGZlTWVyZ2VOb2RlIGluPSJTb3VyY2VHcmFwaGljIj48L2ZlTWVyZ2VOb2RlPgogICAgICAgICAgPC9mZU1lcmdlPgogICAgICAgIDwvZmlsdGVyPgoKICAgICAgIAoKICAgICAgICA8Y2xpcFBhdGggaWQ9ImJhY2stcGxhdGUtY2xpcCI+CiAgICAgICAgICA8cGF0aCBkPSJNNjg0IDQzMkg1MTJ2LTQ5LjE0M0ExMTIgMTEyIDAgMSAwIDQxNiAyNzJhMTExLjU1NiAxMTEuNTU2IDAgMCAwIDEwLjc4NSA0OEgxNjBhMzIuMDk0IDMyLjA5NCAwIDAgMC0zMiAzMnYzMjBhMzIuMDk0IDMyLjA5NCAwIDAgMCAzMiAzMmgxNzguNjdjMTUuMjM2IDkwLjggOTQuMiAxNjAgMTg5LjMzIDE2MCAxMDYuMDM5IDAgMTkyLTg1Ljk2MSAxOTItMTkyVjQ2OGEzNiAzNiAwIDAgMC0zNi0zNnoiIGZpbGw9IiNmZmYiPjwvcGF0aD4KICAgICAgICA8L2NsaXBQYXRoPgogICAgICA8L2RlZnM+CiAgICAgIDxnIGlkPSJzbWFsbF9wZXJzb24iIGZpbHRlcj0idXJsKCNwZXJzb24tc2hhZG93KSI+CiAgICAgICAgPHBhdGggaWQ9IkJvZHkiIGNsYXNzPSJjbHMtMSIgZD0iTTY5MiA0MzJoMTY4YTM2IDM2IDAgMCAxIDM2IDM2djE2NGExMjAgMTIwIDAgMCAxLTEyMCAxMjAgMTIwIDEyMCAwIDAgMS0xMjAtMTIwVjQ2OGEzNiAzNiAwIDAgMSAzNi0zNnoiPjwvcGF0aD4KICAgICAgICA8Y2lyY2xlIGlkPSJIZWFkIiBjbGFzcz0iY2xzLTEiIGN4PSI3NzYiIGN5PSIzMDQiIHI9IjgwIj48L2NpcmNsZT4KICAgICAgPC9nPgogICAgICA8ZyBpZD0iTGFyZ2VfUGVyc29uIiBmaWx0ZXI9InVybCgjcGVyc29uLXNoYWRvdykiPgogICAgICAgIDxwYXRoIGlkPSJCb2R5LTIiIGRhdGEtbmFtZT0iQm9keSIgY2xhc3M9ImNscy0yIiBkPSJNMzcyIDQzMmgzMTJhMzYgMzYgMCAwIDEgMzYgMzZ2MjA0YTE5MiAxOTIgMCAwIDEtMTkyIDE5MiAxOTIgMTkyIDAgMCAxLTE5Mi0xOTJWNDY4YTM2IDM2IDAgMCAxIDM2LTM2eiI+PC9wYXRoPgogICAgICAgIDxjaXJjbGUgaWQ9IkhlYWQtMiIgZGF0YS1uYW1lPSJIZWFkIiBjbGFzcz0iY2xzLTIiIGN4PSI1MjgiIGN5PSIyNzIiIHI9IjExMiI+PC9jaXJjbGU+CiAgICAgIDwvZz4KICAgICAgPHJlY3QgaWQ9IkJhY2tfUGxhdGUiIHg9IjEyOCIgeT0iMzIwIiB3aWR0aD0iMzg0IiBoZWlnaHQ9IjM4NCIgcng9IjMyIiByeT0iMzIiIGZpbHRlcj0idXJsKCNiYWNrLXBsYXRlLXNoYWRvdykiIGNsaXAtcGF0aD0idXJsKCNiYWNrLXBsYXRlLWNsaXApIiBmaWxsPSJ1cmwoI3BsYXRlLWZpbGwpIj48L3JlY3Q+CiAgICAgIDxwYXRoIGlkPSJMZXR0ZXJfVCIgZD0iTTM5OS4zNjUgNDQ1Ljg1NWgtNjAuMjkzdjE2NC4yaC0zOC40MTh2LTE2NC4yaC02MC4wMlY0MTRoMTU4LjczeiIgZmlsdGVyPSJ1cmwoI3RlZS1zaGFkb3cpIiBmaWxsPSIjZmZmIj48L3BhdGg+CiAgICA8L3N2Zz4=";
    const container = <HTMLElement>template.content.cloneNode(true);
    (<HTMLImageElement>container.querySelector(".teams-embed-button-page-btn-img")).src = teamsIcon;
    (<HTMLElement>container.querySelector(".teams-embed-button-page-btn-text")).innerText = this.text;
    if (this.callback) {
      (<HTMLElement>container.querySelector(".teams-embed-button-page-btn")).addEventListener("click", this.callback);
    }
    this.appendChild(container);
  }
}

customElements.define("button-page", ButtonPage);
