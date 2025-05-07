import { useState } from 'react'

export const useExample = (initial: number) => {
    const [count, setCount] = useState(initial)
    const increment = () => setCount(c => c + 4)
    
    return {
        count,
        increment
    }
}