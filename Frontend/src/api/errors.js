/**
 * Frontend Error Handling System.
 * Maps HTTP status codes to user-friendly Spanish messages.
 */

export const ERROR_MESSAGES = {
  // Client errors
  400: 'La solicitud es inválida o faltan datos.',
  401: 'Sesión expirada o no autorizada. Por favor, iniciá sesión nuevamente.',
  403: 'No tenés permisos para realizar esta acción.',
  404: 'El recurso solicitado no existe.',
  408: 'La solicitud tardó demasiado. Por favor, reintentá.',
  409: 'Conflicto: el recurso ya existe o fue modificado por otro usuario.',
  422: 'Los datos proporcionados no son válidos.',
  429: 'Demasiadas solicitudes. Por favor, esperá un momento.',

  // Server errors
  500: 'Ocurrió un error inesperado en el servidor. Por favor, contactá a soporte.',
  502: 'El servidor está temporalmente fuera de servicio.',
  503: 'Servicio no disponible. El servidor podría estar sobrecargado.',
  504: 'Tiempo de respuesta agotado en el servidor.',

  // Fallbacks
  DEFAULT: 'Ocurrió un error inesperado. Por favor, intentá de nuevo.',
  NETWORK: 'No se pudo conectar con el servidor. Verificá tu conexión a internet.',
  TIMEOUT: 'El servidor tardó demasiado en responder.',
}

/**
 * Logic to extract the best possible message based on the error response.
 */
export function getErrorMessage(error) {
  if (!error.response) {
    if (error.code === 'ECONNABORTED') return ERROR_MESSAGES.TIMEOUT
    return ERROR_MESSAGES.NETWORK
  }

  const { status, data } = error.response

  // 1. Try to get a specific error message from the backend if it's a 400 or 409
  // (Business rules often have specific messages we want to show)
  if (status === 400 || status === 409 || status === 422) {
    const backendMsg = data?.error || data?.message
    if (backendMsg) {
      // Optional: you can still have a small translation map for common backend strings
      return translateBackendMessage(backendMsg)
    }
  }

  // 2. Fallback to status-based messages
  return ERROR_MESSAGES[status] || ERROR_MESSAGES.DEFAULT
}

/**
 * Optional: Translate specific backend strings to Spanish
 * if they are not already translated by the backend.
 */
function translateBackendMessage(msg) {
  if (!msg) return msg
  
  const map = {
    'User already exists': 'Este correo ya está registrado.',
    'Invalid credentials': 'Correo o contraseña incorrectos.',
    'User not found': 'Usuario no encontrado.',
    'Seat not available': 'El asiento ya no está disponible.',
    'Seat already taken': 'El asiento acaba de ser reservado por otro usuario.',
    'Reservation has expired': 'La reserva ha expirado.',
    'Invalid status': 'El estado proporcionado no es válido.',
  }

  return map[msg] || msg
}
