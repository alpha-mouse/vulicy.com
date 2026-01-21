const handleResponse = async (response: Response) => {
  if (!response.ok) {
    let errorMessage = `Request failed with status ${response.status}`;
    try {
      const error = await response.json();
      if (error && error.message) {
        errorMessage = error.message;
      }
    } catch {
      // Ignore JSON parse error for error response
    }
    throw new Error(errorMessage);
  }
  return response.json();
};

export const api = {
  get: async <T>(url: string, params?: Record<string, string | number | boolean | undefined | null>): Promise<T> => {
    const urlObj = new URL(url, window.location.origin);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          urlObj.searchParams.set(key, String(value));
        }
      });
    }
    const response = await fetch(urlObj.toString());
    return handleResponse(response);
  },

  post: async <T>(url: string, body?: any): Promise<T> => {
    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse(response);
  },
};
