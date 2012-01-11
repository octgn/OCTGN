import collections

class idict(collections.Mapping):

    def __init__(self, pairs): 
        """Constructor: takes an iterable of tuples""" 
        self._dict = {} 
        for key, value in pairs: 
            k = key.lower() 
            self._dict[k] = (key, value) 

    def __getitem__(self, key):
        k = key.lower()
        pair = self._dict[k]
        return pair[1]

    def __len__(self):
        return len(self._dict)

    def __iter__(self):
        return (key for key, value in self._dict.values()).__iter__()

    def __contains__(self, key):
        k = key.lower()
        return k in self._dict