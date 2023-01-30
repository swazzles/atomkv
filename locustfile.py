from locust import HttpUser, between, task
import random
import string

def get_random_key():
	length = random.randrange(5, 20)
	letters = string.ascii_lowercase
	return ''.join(random.choice(letters) for i in range(length))

class AtomUser(HttpUser):
	def on_start(self):
		self.client.verify = False

	@task
	def putget(self):
		key = get_random_key()
		with self.client.rename_request("/document/{key}"):
			self.client.put("/document/{}".format(key), json = {"doc": "dGVzdA=="})		
			self.client.get("/document/{}".format(key))