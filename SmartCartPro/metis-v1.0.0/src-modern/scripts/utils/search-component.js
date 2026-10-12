// ==========================================================================
// Search component factory — shared shell for the navbar search input.
// Each page passes a `getResults(query)` callback that returns the page's
// matched items (sync or async). Common state (query/results/isLoading)
// and a small simulated-delay are handled here.
// ==========================================================================

const DEFAULT_MIN_LENGTH = 2;
const DEFAULT_DELAY_MS = 200;

export function createSearchComponent({
  getResults,
  minLength = DEFAULT_MIN_LENGTH,
  delayMs = DEFAULT_DELAY_MS,
} = {}) {
  return () => ({
    query: '',
    results: [],
    isLoading: false,

    async search() {
      if (this.query.length < minLength) {
        this.results = [];
        return;
      }
      this.isLoading = true;
      try {
        if (delayMs > 0) {
          await new Promise((resolve) => setTimeout(resolve, delayMs));
        }
        const items = getResults ? await getResults(this.query) : [];
        this.results = Array.isArray(items) ? items : [];
      } finally {
        this.isLoading = false;
      }
    },
  });
}
