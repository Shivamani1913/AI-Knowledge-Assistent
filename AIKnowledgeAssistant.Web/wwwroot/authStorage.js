window.authStorage = {
    setToken: (token) => localStorage.setItem('jwt_token', token),
    getToken: () => localStorage.getItem('jwt_token'),
    setEmail: (email) => localStorage.setItem('user_email', email),
    getEmail: () => localStorage.getItem('user_email'),
    clear: () => { localStorage.removeItem('jwt_token'); localStorage.removeItem('user_email'); }
};
